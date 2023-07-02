using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class MinigameManager_Seahorse : MinigameManager_Core
{
    [SerializeField, FoldoutGroup("Visual"), TableList] private List<PolymorphSpawnableItem> ItemVariations = new();
    [ShowInInspector, ReadOnly, FoldoutGroup("Visual")] private float AliveDuration => 1 / currentDifficulty;

    [SerializeField, FoldoutGroup("Settings"), TableList] private List<AngledAnchor> spawnAnchors = new();

    public override void Awake()
    {
        base.Awake();
        AddonScore.LoadBestScore("Minigame/Seahorse/Best score");
    }

    protected override IEnumerator GameRoutine()
    {
        CheckDecreaseHeartsOnMiss();
        if (isAllowedToRun) yield return new WaitForSeconds(startDelay);
        else yield return new WaitUntil(() => isAllowedToRun);
        
        while (enabled)
        {
            yield return new WaitWhile(() => IsPaused || !isAllowedToRun);

            CheckDecreaseHeartsOnMiss();

            for (int i = 0; i < Random.Range(1, 3); i++) SpawnItems();

            currentDifficulty += difficultyIncreaseOverTime;

            yield return new WaitForSeconds(AliveDuration);
        }
        
        void SpawnItems()
        {
            var freeAnchors = GetFreeAnchors(spawnAnchors.ConvertAll(obj => obj.Anchor));
            if (freeAnchors is null || freeAnchors.Count < 1) 
                return;

            var anchor = freeAnchors[Random.Range(0, freeAnchors.Count)];

            #region Randomizing

            var variants = ItemVariations.ConvertAll(item => item.NestedItem);
            var randomButNestedItem = GetRandomItem(variants);
            var item = ItemVariations.Find(item => item.NestedItem == randomButNestedItem);
            // Get random item from generated list of nested items, and then get back to parent as selected random item with "PolymorphSpawnableItem"'s fields

            #endregion

            var angledAnchor = spawnAnchors.First(obj => obj.Anchor == anchor);

            var angle = angledAnchor.AngleMode switch
            {
                AngledAnchor.AngleDefinitionMode.MinMax => Random.Range(angledAnchor.ZAngleRange.x, angledAnchor.ZAngleRange.y),
                AngledAnchor.AngleDefinitionMode.Array => angledAnchor.ZAngleArray[Random.Range(0, angledAnchor.ZAngleArray.Length)],
                AngledAnchor.AngleDefinitionMode.Transform => angledAnchor.Anchor.localRotation.eulerAngles.z,
                _ => throw new ArgumentOutOfRangeException()
            };

            anchor.transform.localRotation = Quaternion.Euler(0, 0, angle);
            var itemInstance = LeanPool.Spawn(item.NestedItem.Prefab, anchor);
            var itemInstanceLinker = itemInstance.GetComponent<SeahorseMinigame_ItemLinker>();

            var events = itemInstanceLinker.PoolableEvents;

            #region Callback management

            Coroutine lifetimeRoutine = StartCoroutine(Routine());

            events.OnSpawnActions.AddListener(OnSpawn);
            events.OnDespawnActions.AddListener(OnDespawn);

            if (!item.IsTrap)
            {
                // if regular item
                events.OnTriggerActions.AddListener(() => AddScore(item.NestedItem.Value));
                events.OnTriggerActions.AddListener(OnTrigger);
                events.OnNoTriggerActions.AddListener(OnNoTrigger);
            }
            else
            {
                // if trap item (will decrease lives if clicked)
                events.OnDespawnActions.AddListener(() =>
                {
                    if (!events.WasTriggered) AddScore(item.NestedItem.Value);
                });
                events.OnTriggerActions.AddListener(OnNoTrigger);
                events.OnNoTriggerActions.AddListener(OnTrigger);
            }

            events.OnTriggerActions.AddListener(() => events.OnDespawnActions.Invoke());

            events.Init();

            void OnSpawn()
            {
                events.VisibilityToggle.TurnOn();
            }
            void OnDespawn()
            {
                StopCoroutine(lifetimeRoutine);
                events.VisibilityToggle.TurnOff();
                LeanPool.Despawn(itemInstance, 1.5f);
            }
            void OnTrigger()
            {
                //if health enabled - try heal
                if (base.AddonHealth is MinigameManagerAddon_Health_Restorable AddonHealth && decreaseHeartsOnMiss) AddonHealth.TryAdd(1);
            }
            void OnNoTrigger()
            {
                if (base.AddonHealth is MinigameManagerAddon_Health_Restorable AddonHealth)
                {
                    if (decreaseHeartsOnMiss && (item.IsTrap || item.NestedItem.DecreaseLivesOnMiss)) AddonHealth.Damage(1);
                }
            }            
            IEnumerator Routine()
            {
                yield return new WaitForSeconds(AliveDuration);
                // made to support trap items
                if (item.IsTrap == events.WasTriggered) OnNoTrigger();
                events.OnDespawnActions.Invoke();
            }

            #endregion

        }
    }

    private static bool IsAnchorBusy(Transform anchor) => anchor.childCount > 0;

    private static List<Transform> GetFreeAnchors(IEnumerable<Transform> anchors) => anchors.Where(anchor => !IsAnchorBusy(anchor)).ToList();

    [Serializable]
    public class AngledAnchor
    {
        public enum AngleDefinitionMode { MinMax, Array, Transform }

        public Transform Anchor => anchor; [SerializeField] private Transform anchor;
        public AngleDefinitionMode AngleMode => angleMode; [SerializeField] private AngleDefinitionMode angleMode = default;
        [HideInInspector] public Vector2 ZAngleRange => zAngleRange; [SerializeField, ShowIf(nameof(AngleMode), AngleDefinitionMode.MinMax)] private Vector2 zAngleRange = Vector2.zero;
        public float[] ZAngleArray => zAngleArray; [SerializeField, ShowIf(nameof(AngleMode), AngleDefinitionMode.Array)] private float[] zAngleArray;
    }

    [Serializable]
    class PolymorphSpawnableItem
    {
        public Minigame_SpawnableItemSettings NestedItem => _item; [SerializeField, Required] private Minigame_SpawnableItemSettings _item;
        public bool IsTrap => _isTrap; [SerializeField] private bool _isTrap;
    }

    public override void Save()
    {
        var bestScoreKey = "Minigame/Seahorse/Best score";
        var lastBestScore = ES3.Load<int>(bestScoreKey, 0);
        if (AddonScore.Score > lastBestScore) ES3.Save(bestScoreKey, AddonScore.Score);
    }
}