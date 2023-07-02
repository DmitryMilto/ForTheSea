using DG.Tweening;
using Lean.Pool;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class MinigameManager_Turtle : MinigameManager_Core
{
    /// <summary>
    /// Turtle model
    /// </summary>
    [SerializeField, FoldoutGroup("Components")] private RectTransform Player;

    /// <summary>
    /// Oil splashes Spawn area (this GameObject should contain empty children, that will represent "Rows" for player to run on)
    /// </summary>
    [SerializeField, FoldoutGroup("Components")] private RectTransform GameAreaRoot;

    [SerializeField, FoldoutGroup("Components"), Required] private ParallaxScrolling parallax;

    [SerializeField, FoldoutGroup("Settings")] private List<Minigame_SpawnableItemSettings> ItemVariations = new();

    [ShowInInspector, FoldoutGroup("Visual")] private float AliveDuration => defaultAliveDuration / currentDifficulty;
    
    /// <summary>
    /// This list contains Rows of GameAreaRoot. It will be used in Player Controller.
    /// </summary>
    private readonly LinkedList<RectTransform> parsedRows = new();

    [SerializeField, FoldoutGroup("Settings")] private float defaultAliveDuration = 3;

    [SerializeField, FoldoutGroup("Settings")] float playerRotationOffset = 30f, animationDuration = 0.5f;
    private LinkedListNode<RectTransform> playerPosition;
    private NavigationMoveEvent.Direction lastMoveDirection;
    private LinkedListNode<RectTransform> MoveToPositionNode => lastMoveDirection is NavigationMoveEvent.Direction.Up ? playerPosition.Previous : playerPosition.Next;
    private Vector2 MoveToPosition => GetTargetPosition(MoveToPositionNode?.Value, Player.position);
    private Vector3 MoveToRotation => Vector3.forward * playerRotationOffset * (lastMoveDirection is NavigationMoveEvent.Direction.Up ? 1 : -1);
    private List<MinigameItem_Turtle_PoolableEvents> instancesLinkers = new();

    public override void Awake()
    {
        base.Awake();

        InitializeParsedRows();
        SetPlayerPosition();

        void InitializeParsedRows()
        {
            for (int i = 0; i < GameAreaRoot.childCount; i++)
            {
                parsedRows.AddLast(GameAreaRoot.GetChild(i) as RectTransform);
            }
        }
        void SetPlayerPosition()
        {
            playerPosition = parsedRows.Find(Player.parent as RectTransform);
            Player.SetParent(GameAreaRoot, true);
            Player.DOAnchorPosY(0, 0.01f, true);
        }

        AddonScore.LoadBestScore("Minigame/Turtle/Best score");
    }

    void OnDisable()
    {
        instancesLinkers.ForEach(linker => linker?.MoveTweener?.Kill());
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

            SpawnItem(AliveDuration);
            currentDifficulty += difficultyIncreaseOverTime;

            yield return new WaitForSeconds(AliveDuration * 0.33f);
        }

        void SpawnItem(float aliveDuration)
        {
            // randomize spawn settings
            int spawnRowIndex = Random.Range(0, parsedRows.Count);
            var spawnRowRaw = parsedRows.ElementAt(spawnRowIndex).transform;
            if (spawnRowRaw is not RectTransform spawnRow) throw new NullReferenceException();
            var item = GetRandomItem(ItemVariations);

            // create instance of randomly selected GameObject
            var obj = LeanPool.Spawn(item.Prefab, spawnRow);

            // if spawner capacity reached - it won't instantiate provided GameObject
            if (obj is null)
            {
                Debug.LogWarning("Spawner returned null GameObject. May it due Spawner capacity limit reaching?");
                return;
            }

            if (obj.transform is not RectTransform objectsRectTransform) throw new NullReferenceException();

            // initialize item properties and callbacks
            MinigameItem_Turtle_PoolableEvents objLinker;
            instancesLinkers.Add(objLinker = obj.GetComponent<MinigameItem_Turtle_PoolableEvents>());
            objLinker.OnSpawnActions.AddListener(OnSpawn);
            objLinker.OnDespawnActions.AddListener(OnDespawn);
            objLinker.OnTriggerActions.AddListener(OnTrigger);
            objLinker.OnNoTriggerActions.AddListener(OnNoTrigger);

            objLinker.OnSpawnActions.Invoke();

            void OnSpawn()
            {
                objLinker.VisibilityToggle.TurnOn();
                objLinker.MoveTweener = obj.transform
                    .DOMoveX(GetEndPosition(spawnRow).x, aliveDuration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => objLinker.OnNoTriggerActions.Invoke());

                Vector2 GetEndPosition(RectTransform spawnRow)
                {
                    Vector2 rawEndPosition = spawnRow.TransformPoint(spawnRow.pivot - new Vector2(spawnRow.rect.xMax, 0));
                    var offset = objectsRectTransform.rect.width * obj.transform.root.lossyScale.x;
                    var endPosition = rawEndPosition - new Vector2(offset, 0);
                    return endPosition;
                }
            }
            void OnDespawn()
            {
                LeanPool.Despawn(obj, objLinker.WasTriggered ? objLinker.HidingDuration : 0);
            }
            void OnTrigger()
            {
                // if not trap
                if (item.DecreaseLivesOnMiss)
                {
                    if (base.AddonHealth is MinigameManagerAddon_Health_Restorable AddonHealth && decreaseHeartsOnMiss) AddonHealth.TryAdd(1);
                    AddScore(item.Value);
                    objLinker.MoveTweener.onComplete = null;
                    objLinker.VisibilityToggle.TurnOff();
                    objLinker.MoveTweener.timeScale = 1 / aliveDuration;
                    objLinker.OnDespawnActions.Invoke();
                }
                else
                {
                    // if trap
                    if (base.AddonHealth is MinigameManagerAddon_Health AddonHealth && decreaseHeartsOnMiss) AddonHealth.Damage(1);
                    objLinker.OnDespawnActions.Invoke();
                }
            }
            void OnNoTrigger()
            {
                // if not trap
                if (item.DecreaseLivesOnMiss)
                {
                    if (base.AddonHealth is MinigameManagerAddon_Health AddonHealth && decreaseHeartsOnMiss) AddonHealth.Damage(1);
                }
                objLinker.OnDespawnActions.Invoke();
            }
        }
    }

    public override void Save()
    {
        var bestScoreKey = "Minigame/Turtle/Best score";
        var lastBestScore = ES3.Load<int>(bestScoreKey, 0);
        if (AddonScore.Score > lastBestScore) ES3.Save(bestScoreKey, AddonScore.Score);
    }

    public void MovePlayer(string direction)
    {
        switch (direction)
        {
            case "Up": MovePlayer(NavigationMoveEvent.Direction.Up); break;
            case "Down": MovePlayer(NavigationMoveEvent.Direction.Down); break;
            default: throw new ArgumentOutOfRangeException(nameof(direction), $"Direction doesn't match predefined cases. (Was: \"{direction}\")");
        }
    }
    private void MovePlayer(NavigationMoveEvent.Direction direction)
    {
        switch (direction)
        {
            case NavigationMoveEvent.Direction.Up when playerPosition.Previous?.Value is null:
            case NavigationMoveEvent.Direction.Down when playerPosition.Next?.Value is null:
                return;
        }

        lastMoveDirection = direction;

        Move(MoveToPosition);
        Rotate(MoveToRotation);

        void Move(Vector3 position)
        {
            Player
                .DOMoveY(position.y, animationDuration)
                .SetEase(Ease.InOutSine)
                //.OnComplete(OnMoveComplete)
                //.SetAutoKill(false)
                ;
            OnMoveComplete();
            
            void OnMoveComplete() => playerPosition = MoveToPositionNode;
        }
        void Rotate(Vector3 direction)
        {
            // Bug (?): Rotation on swipe during rotation tween being in progress results rotation zero-ing, that might be unwanted.

            Player
                .DOLocalRotate(direction, animationDuration / 2)
                .SetEase(Ease.InSine)
                .OnComplete(RotateToZero)
                //.SetAutoKill(false)
                ;

            void RotateToZero() => 
                Player
                    .DOLocalRotate(Vector3.zero, animationDuration / 2)
                    .SetEase(Ease.OutSine);
        }
    }
    static Vector2 GetTargetPosition(RectTransform target, Vector2 PlayerPosition) => new(PlayerPosition.x, target.position.y);
}
