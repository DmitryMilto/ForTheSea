using Lean.Localization;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public abstract class MinigameManager_Core : MonoBehaviour, IMinigameSavable
{
    [SerializeField, FoldoutGroup("General"), PropertyOrder(-1)] protected bool isAllowedToRun = true;
    [SerializeField, FoldoutGroup("General"), ShowIf(nameof(CheckIf_HealthAssigned)), PropertyOrder(-1)] protected bool decreaseHeartsOnMiss = true;
    [SerializeField, FoldoutGroup("General"), SuffixLabel("sec", true), PropertyOrder(-1)] protected float startDelay = 2f;

    [SerializeField, FoldoutGroup("General/Difficulty"), SuffixLabel("Multiplier", true)] protected float currentDifficulty = 1;
    [SerializeField, FoldoutGroup("General/Difficulty"), SuffixLabel("(Additive)")] protected float difficultyIncreaseOverTime = 0.05f;

    public MinigameManagerAddon_Score AddonScore => addonScore; [FoldoutGroup("Addons"), SerializeField, PropertyOrder(1)] protected MinigameManagerAddon_Score addonScore;
    public MinigameManagerAddon_Timer AddonTimer => addonTimer; [FoldoutGroup("Addons"), SerializeField, PropertyOrder(1)] protected MinigameManagerAddon_Timer addonTimer;
    public MinigameManagerAddon_HealthBasic AddonHealth => addonHealth; [FoldoutGroup("Addons"), SerializeField, PropertyOrder(1)] protected MinigameManagerAddon_HealthBasic addonHealth;

    [FoldoutGroup("Events"), PropertyOrder(1)] public UnityEvent<bool> OnPause, Running;

    [ShowInInspector, ReadOnly, PropertyOrder(1)] public bool IsPaused { get; private set; } = false;
    [ShowInInspector, ReadOnly, PropertyOrder(1)] public bool IsRunning => loop is not null && isAllowedToRun && IsPaused is not true;

    protected Coroutine loop;
    private IMinigameSavable minigameSavableImplementation;

    public virtual void Awake()
    {
        OnPause.AddListener(OnPausedChanged);
        Running.AddListener(OnRunningChanged);
        AddonHealth?.OnDeath.AddListener(() =>
        {
            OnPause.Invoke(false);
            Running.Invoke(false);
            Save();
        });
    }

    public virtual void OnDestroy()
    {
        AddonHealth?.OnDeath.RemoveListener(Save);
    }

    protected virtual void Start()
    {
        Running.Invoke(true);
        LeanLocalization.UpdateTranslations();
    }
    private void OnPausedChanged(bool value)
    {
        IsPaused = value;
        Time.timeScale = IsPaused ? 0 : 1;
    }
    private void OnRunningChanged(bool value)
    {
        if (value)
        {
            loop = StartCoroutine(GameRoutine());
        }
        else
        {
            if (loop is not null) StopCoroutine(loop);
        }
    }
    protected abstract IEnumerator GameRoutine();
    protected virtual void CheckDecreaseHeartsOnMiss()
    {
        // If state of heart behaviour changed - influence visibility of the HUD
        if (this.AddonHealth is MinigameManagerAddon_Health_Restorable AddonHealth && decreaseHeartsOnMiss != AddonHealth.HealthVisibility.On) AddonHealth.HealthVisibility.Set(decreaseHeartsOnMiss);
    }
    private static int chanceSum = 0;
    public static Minigame_SpawnableItemSettings GetRandomItem(List<Minigame_SpawnableItemSettings> items)
    {
        //// Get Random chance
        //var rarity = Random.Range(0, 1f);
        //Debug.Log($"random value {rarity}");
        //// Filter to get valid pool of items
        //var validItems = items
        //    .Where(item => rarity <= item.SpawnChance)
        //    .OrderBy(item => item.SpawnChance)
        //    .ToList(); 

        //var minimumItem = validItems.First().SpawnChance;

        //// get items with minimum chance
        //validItems = validItems.Where(item => item.SpawnChance == minimumItem).ToList();

        ////Get random item, from items, with equal spawn chance
        //var item = validItems[Random.Range(0, validItems.Count)];

        //return item;

        Init(items);
        int rand = Random.Range(0, chanceSum);
        foreach (var item in items)
        {
            if (rand >= item.minValue && rand < item.maxValue)
            {
                return item;
            }
        }
        return null; 
    }
    public static void Init(List<Minigame_SpawnableItemSettings> list)
    {
        var chance = 0;
        chanceSum = 0;
        foreach (var item in list)
        {
            chanceSum += item.changeRandom;
            item.minValue = chance;
            item.maxValue = chance += item.changeRandom;
        }
    }
    public static GameObject GetRandomItem(List<GameObject> items) => items.ElementAt(Random.Range(0, items.Count));

    public void SetPaused(bool value) => OnPause.Invoke(value);
    protected virtual void OnApplicationFocus(bool hasFocus)
    {
        if (!IsPaused)
            Time.timeScale = !hasFocus ? 0 : 1;
    }
    protected virtual void OnApplicationPause(bool pauseStatus)
    {
        if(!IsPaused)
            Time.timeScale = pauseStatus ? 0 : 1;
    } 

    public static List<Transform> ListChildren(Transform source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (source.childCount == 0) return new List<Transform>();

        List<Transform> children = new();
        for (int i = 0; i < source.childCount; i++) children.Add(source.GetChild(i));

        return children;
    }

    protected bool CheckIf_HealthAssigned() => AddonHealth is MinigameManagerAddon_Health;
    public abstract void Save();

    public virtual void AddScore(int value)
    {
        if (IsRunning is false || AddonScore is null || AddonHealth is null || AddonHealth.IsDead) return;
        AddonScore.Score += value;
    }

    public void AllowStart(bool value = true) => isAllowedToRun = value;
}
