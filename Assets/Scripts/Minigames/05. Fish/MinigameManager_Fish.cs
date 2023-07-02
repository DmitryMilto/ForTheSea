using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class MinigameManager_Fish : MinigameManager_Core
{
    [SerializeField, FoldoutGroup("Settings"), TableList] private List<Coral> corals;
    [SerializeField, FoldoutGroup("Settings")] private float rotationDuration = 0.5f;
    [SerializeField, FoldoutGroup("Settings")] private Transform parentForRandom;

    [SerializeField, FoldoutGroup("Components")] private Transform player;
    [SerializeField, FoldoutGroup("Components")] private SceneStopwatch stopwatch;
    private Animator PlayerAnimator => _animator ?? player.GetComponent<Animator>(); [SerializeField, FoldoutGroup("Components"), ShowIf("@_animator == null")] private Animator _animator;

    [SerializeField, FoldoutGroup("Components/Visual")] private Transform playerOnHoldVFXRoot;
    [SerializeField, FoldoutGroup("Components/Visual")] private UnityEvent<bool> OnExhale;

    [SerializeField, FoldoutGroup("Addons"), PropertyOrder(1)] private MinigameManagerAddon_Stamina addonStamina;

    public override void Awake()
    {
        base.Awake();
        AddonTimer.LoadBestScore("Minigame/Fish/Best score");
        RandomCoralInstance();
    }

    void OnEnable()
    {
        corals.ForEach(coral => coral.Button?.OnIsPressedChanged?.AddListener(OnCoralPressed));
        OnPause?.AddListener(value =>
        {
            CoralsChangeAllowed(!value);
            stopwatch.PauseResumeStopwatch(value); //RunStopwatch(!value);
        });
        AddonHealth?.OnDeath.AddListener(() =>
        {
            CoralsChangeAllowed(false);
            stopwatch.PauseResumeStopwatch(true);
        });
    }

    private void CoralsChangeAllowed(bool value) => corals.ForEach(coral => coral.Health.IsChangeAllowed = value);

    protected override IEnumerator GameRoutine()
    {
        CheckDecreaseHeartsOnMiss();
        if (isAllowedToRun) yield return new WaitForSeconds(startDelay);
        else yield return new WaitUntil(() => isAllowedToRun);
        stopwatch.StartStopwatch();

        while (enabled)
        {
            yield return new WaitWhile(() => IsPaused || !isAllowedToRun);

            CheckDecreaseHeartsOnMiss();

            AddonTimer.Score = stopwatch.Duration;
            currentDifficulty += difficultyIncreaseOverTime;

            foreach (var coral in corals)
            {
                coral.OnTimeChanged(stopwatch.Duration);
                if (coral.IsDecreasingAllowed) coral.OnDifficultyChange(currentDifficulty / 100);
            }

            if (corals.Any(coral => coral.Health.Value == 0)) AddonHealth?.OnDeath.Invoke();

            yield return new WaitForSeconds(1);
        }
    }

    public override void Save()
    {
        var bestScoreKey = "Minigame/Fish/Best score";
        var lastBestScore = ES3.Load<TimeSpan>(bestScoreKey, TimeSpan.Zero);
        if (AddonTimer.Score > lastBestScore) ES3.Save(bestScoreKey, AddonTimer.Score);
    }

    private void OnCoralPressed(bool value)
    {
        if (value)
        {
            Vector3 targetPosition = corals.First(coral => coral.Button.IsPressed).Instance.transform.position;
            ProjectCustomTools.Rotate2DImageTowards(player, targetPosition, rotationDuration);
            DOTween.To(() => playerOnHoldVFXRoot.right, value => playerOnHoldVFXRoot.right = value, (targetPosition - player.position).normalized, rotationDuration);
        }

        if (value && !addonStamina.IsOnCooldownRestore)
        {
            OnExhale.Invoke(true);
            //playerOnHoldVFX.Play();
            //addonStamina.IsTriggered = true;
            PlayerAnimator.SetBool("IsBlowing", true);
            StartCoroutine(StaminaTracker());
        }
        else
        {
            OnExhale.Invoke(false);
            //playerOnHoldVFX.Stop();
            //addonStamina.IsTriggered = false;
            PlayerAnimator.SetBool("IsBlowing", false);
            StopCoroutine(StaminaTracker());
        }
    }
    private IEnumerator StaminaTracker()
    {
        while (addonStamina.IsTriggered/*playerOnHoldVFX.isPlaying*/)
        {
            yield return new WaitUntil(() => addonStamina.IsOnCooldownRestore);
            corals.Where(coral => coral.Button.IsPressed).ToList().ForEach(coral => coral.Button.OnIsPressedChanged.Invoke(false));
        }
    }

    public void RotatePlayer(string side) => PlayerAnimator.SetTrigger("Move to " + side);
    public void ResetPlayerRotationSetter(string side) => PlayerAnimator.ResetTrigger("Move to " + side);

    [Serializable]
    public class Coral
    {
        [HideInInspector] public GameObject Instance => _instance; 
        [SerializeField] private GameObject _instance;

        [ShowInInspector, ReadOnly, HideIf("@Instance == null || _button == null")] 
        public HoldableButton Button => _button ?? (Instance is null ? null : _button = Instance.GetComponentInChildren<HoldableButton>())/*Instance is null ? null : _button ?? Instance.GetComponentInChildren<HoldableButton>()*/; 
        private HoldableButton _button;
        
        [ShowInInspector, ReadOnly, HideIf("@Instance == null || _health == null")] 
        public CoolableCoral Health => _health ?? (Instance is null ? null : _health = Instance.GetComponentInChildren<CoolableCoral>())/*Instance is null ? null : _health ?? Instance.GetComponentInChildren<CoolableCoral>()*/; 
        private CoolableCoral _health;
      
        [InfoBox("Please, use next format: \"0:00:00\" to parse value correctly", InfoMessageType.Warning, "@FreezeTimeThreshold.TotalHours >= 1")]
        [InfoBox("Invalid value", InfoMessageType.Error, "@IsTimeSpanInvalid(_freezeTimeThreshold)")]
        [SerializeField] private string _freezeTimeThreshold = "0:00:00"; 
        public TimeSpan FreezeTimeThreshold => TimeSpan.TryParse(_freezeTimeThreshold, out TimeSpan value) is false ? TimeSpan.Zero : value;
        [ShowInInspector, HideIf("@Instance == null || _health == null")] public bool IsDecreasingAllowed
        {
            get => Instance is not null && (Health.IsChangeAllowed && Health.IsDecreasingAllowed);
            set
            {
                if (Instance is not null) Health.IsChangeAllowed = Health.IsDecreasingAllowed = value;
            }
        }

        public void OnTimeChanged(TimeSpan time)
        {
            if (FreezeTimeThreshold > time || IsDecreasingAllowed is true) return;
            IsDecreasingAllowed = true;
        }

        public void OnDifficultyChange(float newDifficultyMultiplier)
        {
            Health.DecreaseRate += newDifficultyMultiplier;
            Health.IncreaseRate += newDifficultyMultiplier / 2;
        }
        public void UpdateCoral(GameObject coral)
        {
            _instance = coral;
            _button = Instance.GetComponentInChildren<HoldableButton>();
            _health = Instance.GetComponentInChildren<CoolableCoral>();
            IsDecreasingAllowed = false;
        }

        internal static bool IsTimeSpanInvalid(string valueRaw) => TimeSpan.TryParse(valueRaw, out _) is false;
    }
    [Button]
    private void RandomCoralInstance()
    {
        var list = parentForRandom.GetComponentsInChildren<CoolableCoral>().ToList();
        list.Shuffle();
        for(int i = 0; i < list.Count; i++)
        {
            corals[i].UpdateCoral(list[i].gameObject);
            if (i == 0)
                corals[i].IsDecreasingAllowed = true;
        }
    }
}
public static class ThreadSafeRandom
{
    [ThreadStatic] private static System.Random Local;

    public static System.Random ThisThreadsRandom
    {
        get { return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
    }
}
static class Ramdoms
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
