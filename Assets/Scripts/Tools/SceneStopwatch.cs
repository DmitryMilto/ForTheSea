using Lean.Transition;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class SceneStopwatch : MonoBehaviour
{
    [FoldoutGroup("State info"), ShowInInspector, DisplayAsString, SuffixLabel("HH:MM:SS", true)] public TimeSpan Duration => TimeSpan.FromSeconds(duration); [SerializeField, HideInInspector] private float duration = 0;
    [FoldoutGroup("State info"), ShowInInspector] public bool IsPaused => isPaused || (Application.isFocused is false && pauseWhenAppNotFocused); [SerializeField, HideInInspector] private bool isPaused = false;
    [FoldoutGroup("State info"), ShowInInspector] public bool IsRunning => IsPaused is false && tracker is not null;
    [FoldoutGroup("State info"), ShowInInspector] public bool IsStopped => tracker is null;

    [FoldoutGroup("Settings"), SerializeField, InfoBox("Time scale may be delayed by one scale step", InfoMessageType.Warning,VisibleIf = nameof(delayedTimeScales))] private LeanTiming timeScale = LeanTiming.Default;
    [FoldoutGroup("Settings"), SerializeField, ToggleLeft] private bool pauseWhenAppNotFocused = true;

    [FoldoutGroup("Events")] public UnityEvent OnStart, OnStop;
    [FoldoutGroup("Events")] public UnityEvent<float> OnValueChanged;

    private Coroutine tracker;

    private bool delayedTimeScales => timeScale is 
        LeanTiming.UnscaledFixedUpdate 
        or LeanTiming.UnscaledLateUpdate
        or LeanTiming.UnscaledUpdate;

    [Button, DisableIf(nameof(IsRunning)), ShowIf("@UnityEngine.Application.isPlaying"), HorizontalGroup("Controls"), LabelText("Start")] public void StartStopwatch()
    {
        ResetStopwatch();
        tracker = StartCoroutine(Routine());
        OnStart.Invoke();
    }

    [Button, DisableIf(nameof(IsStopped)), ShowIf("@UnityEngine.Application.isPlaying"), HorizontalGroup("Controls"), LabelText("Stop")] public void StopStopwatch()
    {
        StopCoroutine(tracker);
        isPaused = false;
        tracker = null;
        OnStop.Invoke();
    }

    [Button, DisableIf(nameof(IsStopped)), ShowIf("@UnityEngine.Application.isPlaying"), HorizontalGroup("Controls"), LabelText("Pause/Resume")] public void PauseResumeStopwatch(bool? forceSetToState = null)
    {
        if (IsStopped) StartStopwatch();
        else isPaused = forceSetToState ?? !isPaused;
    }

    [Button, DisableIf(nameof(IsRunning)), ShowIf("@UnityEngine.Application.isPlaying"), HorizontalGroup("Controls"), LabelText("Reset")] public void ResetStopwatch()
    {
        duration = 0;
        isPaused = false;
    }

    private IEnumerator Routine()
    {
        while (enabled)
        {
            yield return new WaitWhile(() => IsPaused);
            switch (timeScale)
            {
                case LeanTiming.UnscaledFixedUpdate: {
                    yield return new WaitForSecondsRealtime(Time.fixedUnscaledDeltaTime);
                    duration += Time.fixedUnscaledDeltaTime;
                } break;
                case LeanTiming.UnscaledLateUpdate:
                case LeanTiming.UnscaledUpdate: {
                    yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
                    duration += Time.unscaledDeltaTime;
                } break;
                case LeanTiming.Default:
                case LeanTiming.Update:
                case LeanTiming.LateUpdate: {
                    yield return new WaitForEndOfFrame();
                    duration += Time.deltaTime;
                } break;
                case LeanTiming.FixedUpdate: {
                    yield return new WaitForFixedUpdate();
                    duration += Time.fixedDeltaTime;
                } break;
                default: throw new ArgumentOutOfRangeException();
            }
            OnValueChanged.Invoke(duration);
        }
    }

    public void LogDuration() => Debug.Log(Duration, this);
}
