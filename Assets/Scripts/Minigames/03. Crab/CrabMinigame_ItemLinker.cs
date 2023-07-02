using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Lean.Gui;
using Lean.Transition;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class CrabMinigame_ItemLinker : MonoBehaviour
{
    enum SliceableStateDetectionModes { LeanToggle, AnimatorsCurrentState }

    [SerializeField, FoldoutGroup("Settings")] private float onSwipe_disapearDuration = 2f;
    [SerializeField, FoldoutGroup("Settings")] private Ease moveBehaviourAfterSwipe = Ease.InOutSine; //Ease.OutCubic
    [SerializeField, FoldoutGroup("Settings")] private SliceableStateDetectionModes stateDetectionMode = SliceableStateDetectionModes.LeanToggle;
    [SerializeField, FoldoutGroup("Settings"), ShowIf(nameof(stateDetectionMode), SliceableStateDetectionModes.AnimatorsCurrentState)] private bool swipeMakesCreatureHappy = true;

    [SerializeField, FoldoutGroup("Visual")] private LeanToggle visibilityToggle;
    [SerializeField, FoldoutGroup("Visual"), ShowIf(nameof(stateDetectionMode), SliceableStateDetectionModes.LeanToggle)] private LeanToggle sliceableToggle;
    [SerializeField, FoldoutGroup("Visual"), ShowIf(nameof(stateDetectionMode), SliceableStateDetectionModes.AnimatorsCurrentState)] private Animator animator;
    [SerializeField, FoldoutGroup("Visual"), ShowIf(nameof(stateDetectionMode), SliceableStateDetectionModes.AnimatorsCurrentState)] private CreaturesAnimatorStates animatorStates;

    public RectTransform RotatableVisual => rotatableVisual; [SerializeField, FoldoutGroup("Components")] private RectTransform rotatableVisual;

    [SerializeField] private LeanAnimationRepeater despawnTimer;

    [FoldoutGroup("Events")] public UnityEvent OnSpawn, OnDespawn, OnSwipe;
    [FoldoutGroup("Events/Swipe result")] public UnityEvent OnSwipeOk, OnSwipeBad;
    private MinigameManager_Crab MinigameManager
    {
        get
        {
            var candidate = FindObjectOfType<MinigameManager_Crab>();
            return candidate is null ? throw new MissingComponentException() : minigameManager ??= candidate;
        }
        set => minigameManager = value ?? throw new ArgumentNullException(nameof(value));
    } MinigameManager_Crab minigameManager;

    private TweenerCore<Vector3, Vector3, VectorOptions> moveTween;

    private Minigame_SpawnableItemSettings data;
    private bool DamageOnMiss => data.DecreaseLivesOnMiss;
    private bool WasHit
    {
        get
        {
            AnimatorStateInfo currentState; 
            switch (stateDetectionMode)
            {
                case SliceableStateDetectionModes.LeanToggle: return !sliceableToggle.On;
                case SliceableStateDetectionModes.AnimatorsCurrentState:
                    currentState = animator.GetCurrentAnimatorStateInfo(0);
                    if (animatorStates.BadStates.Any(state => currentState.IsName(state.name))) return !swipeMakesCreatureHappy;
                    if (animatorStates.OkStates.Any(state => currentState.IsName(state.name))) return swipeMakesCreatureHappy;
                    throw new ArgumentOutOfRangeException();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        set
        {
            switch (stateDetectionMode)
            {
                case SliceableStateDetectionModes.LeanToggle: sliceableToggle.Set(!value); break;
                case SliceableStateDetectionModes.AnimatorsCurrentState:
                    if (value) animator.SetTrigger(swipeMakesCreatureHappy ? "Happy" : "Sad");
                    else ResetCreatureState();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void ResetCreatureState()
    {
        var randomInt = Random.Range(0, swipeMakesCreatureHappy ? animatorStates.BadStates.Length : animatorStates.OkStates.Length);
        var separation = (swipeMakesCreatureHappy ? animatorStates.BadStates[randomInt] : animatorStates.OkStates[randomInt]).name.Split(' ');
        var triggerName = separation.Last();
        animator.SetTrigger(triggerName);
    }


    private void OnEnable()
    {
        OnSpawn.AddListener(OnSpawnActions);
        OnDespawn.AddListener(OnDespawnActions);
        OnSwipe.AddListener(OnSwipeActions);
    }
    private void OnDisable()
    {
        OnSpawn.RemoveListener(OnSpawnActions);
        OnDespawn.RemoveListener(OnDespawnActions);
        OnSwipe.RemoveListener(OnSwipeActions);
    }

    public void Init(float duration, Vector2 destination, Minigame_SpawnableItemSettings itemData)
    {
        data = itemData;

        OnSpawn.Invoke();
        moveTween = transform.DOMoveX(destination.x, duration);
        SetDespawnTimer(duration);
    }

    private void OnSpawnActions()
    {
        MinigameManager.instantiatedItems.Add(this, gameObject);
        OnSwipeOk.AddListener(AddScore);
        ResetCreatureState();
    }
    
    private void OnDespawnActions()
    {
        MinigameManager.instantiatedItems.Remove(this);
        OnSwipeOk.RemoveListener(AddScore);
        if (!WasHit && DamageOnMiss) minigameManager.DamageHP();
        ResetInstance();
    }
    private void AddScore() => MinigameManager.IncreaseScore(data.Value);

    private void OnSwipeActions()
    {
        if (!WasHit && DamageOnMiss)
        {
            minigameManager.AddHPProgress();
            OnSwipeOk.Invoke();
        }
        else
        {
            minigameManager.DamageHP();
            OnSwipeBad.Invoke();
        }
        moveTween
            .ChangeValues(transform.position, moveTween.endValue, onSwipe_disapearDuration)
            .SetEase(moveBehaviourAfterSwipe);
        SetDespawnTimer(onSwipe_disapearDuration);
        WasHit = true;
    }

    private void ResetInstance()
    {
        OnSpawn.RemoveAllListeners();
        OnDespawn.RemoveAllListeners();
        OnSwipe.RemoveAllListeners();
        ResetDespawnTimer();
        WasHit = false;

        void ResetDespawnTimer()
        {
            despawnTimer.enabled = false;
            despawnTimer.RemainingCount = 1;
            despawnTimer.TimeInterval = 1;
            despawnTimer.OnAnimation.RemoveListener(OnDespawn.Invoke);
        }
    }

    private void SetDespawnTimer(float value)
    {
        despawnTimer.RemainingCount = 1;
        despawnTimer.RemainingTime = value;
        despawnTimer.TimeInterval = value;
        despawnTimer.enabled = true;
        despawnTimer.OnAnimation.AddListener(OnDespawn.Invoke);
    }

    [Serializable]
    public class CreaturesAnimatorStates
    {
        public AnimationClip[] BadStates => badStates; [SerializeField] private AnimationClip[] badStates;
        public AnimationClip[] OkStates => okStates; [SerializeField] private AnimationClip[] okStates;
    }
}
