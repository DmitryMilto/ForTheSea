using Lean.Gui;
using Michsky.UI.ModernUIPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Video;


public class SplashScreenBehaviour : MonoBehaviour
{
    private readonly List<SequencedBehaviourState> states = new ();
    private int currentState = -1;

    [SerializeField] internal ProgressBar progressBar;
    [SerializeField] internal ToggleWrapper progressBarToggle;

    [Header("Splash video")] 
    [SerializeField] private string splashLogoVideoPath = @"Splash/Logo Splash Screen 16-9";
    [SerializeField] private ToggleWrapper playerVisibilityToggle;
    [SerializeField] private VideoPlayer playbackPlayer;
    [SerializeField] private LeanButton videoplayerSkipButton;

    public bool skipSplash { get; set; } = false;
    [SerializeField] private bool hasSave;

    [Header("Main level")] 
    [SerializeField] private SceneReference MainLevelReference;

    private void Awake()
    {
        // Init SaveSystem
        ES3.Init();
        // Init states
        if (skipSplash is false) states.Add(new ShowSplashVideo(this, TryStartNextState));
        states.Add(new LoadMainLevel(this));
    }

    private void Start()
    {
        // When states set, try to launch the sequence
        TryStartNextState();
    }

    private void TryStartNextState()
    {
        if (currentState + 1 >= states.Count) return;
        states[++currentState].Start();
    }

    private class ShowSplashVideo : SequencedBehaviourState
    {
        public string SplashLogoVideoPath { get; private set; } = @"Splash/Logo Splash Screen 16-9";
        private readonly VideoPlayer PlaybackPlayer;
        private readonly ToggleWrapper PlayerVisibilityToggle_Wrapped;

        public ShowSplashVideo(SplashScreenBehaviour invoker, UnityAction OnTaskComplitedCallback = null) : base(invoker)
        {
            PlaybackPlayer = ctx.playbackPlayer;
            PlayerVisibilityToggle_Wrapped = ctx.playerVisibilityToggle;
            if (OnTaskComplitedCallback is not null) OnTaskComplited.AddListener(OnTaskComplitedCallback);
            if (string.IsNullOrEmpty(ctx.splashLogoVideoPath) is false) SplashLogoVideoPath = ctx.splashLogoVideoPath;
        }
        protected override IEnumerator Routine()
        {
            // Load Splash video from Resources
            var videoRequest = Resources.LoadAsync<VideoClip>(SplashLogoVideoPath);
            ProgressBarToggle.Toggle.TurnOn();

            // Wait for load and update progress bar
            while (!videoRequest.isDone)
            {
                var videoLoadingProgress = videoRequest.progress;
                UpdateProgressBarValue(videoLoadingProgress);
                yield return ProgressBarValue < 1;
            }

            // Activate VideoPlayer GameObject and set clip for preparing
            PlaybackPlayer.gameObject.SetActive(true);
            PlaybackPlayer.clip = videoRequest.asset as VideoClip;
            PlaybackPlayer.Prepare();

            // Wait for player to be ready
            yield return new WaitUntil(() => PlaybackPlayer.isPrepared);

            // Hide Loading Progressbar
            ProgressBarToggle.Toggle.TurnOff();

            // Fade VideoPlayer GameObject In and wait for animations to complete
            PlayerVisibilityToggle_Wrapped.Toggle.TurnOn();
            yield return new WaitForSeconds(PlayerVisibilityToggle_Wrapped.TransitionFadeInLength);

            // Playback the splash video
            PlaybackPlayer.Play();

            // Check for existing save (If save exists - splash will be skippable on tap)
            var saveKey = "General/Last launch";
            ctx.videoplayerSkipButton.interactable = ctx.hasSave = ES3.KeyExists(saveKey);
            ES3.Save(saveKey, DateTime.Now);

            // Wait while Video is playing or, in case of existing save, wait for user to tap
            while (PlaybackPlayer.isPlaying)
            {
                var videoShowProgress = (float)(PlaybackPlayer.time / PlaybackPlayer.length);
                UpdateProgressBarValue(videoShowProgress);
                if (ctx.skipSplash) break;
                yield return ProgressBarValue < 1;
            }

            // Fade VideoPlayer GameObject Out and wait for animations to complete
            PlayerVisibilityToggle_Wrapped.Toggle.TurnOff();
            yield return new WaitForSeconds(PlayerVisibilityToggle_Wrapped.TransitionFadeOutLength);

            // Deactivate VideoPlayer GameObject, free video resource, Invoke event upon completion
            PlaybackPlayer.gameObject.SetActive(false);
            Resources.UnloadAsset(videoRequest.asset);
            OnTaskComplited.Invoke();
            Debug.Log($"Sequence \"{nameof(ShowSplashVideo)}\" ended.");
        }
    }

    private class LoadMainLevel : SequencedBehaviourState
    {

        public LoadMainLevel(SplashScreenBehaviour invoker) : base(invoker)
        {
            var requestedSceneName = ctx.MainLevelReference;
        }
        protected override IEnumerator Routine()
        {
            var requestedSceneBuildIndex = SceneUtility.GetBuildIndexByScenePath(ctx.MainLevelReference.ScenePath);
            var loadSceneRequest = SceneManager.LoadSceneAsync(requestedSceneBuildIndex, LoadSceneMode.Single);
            loadSceneRequest.allowSceneActivation = false;

            ProgressBarToggle.Toggle.TurnOn();
            while (loadSceneRequest.progress < 0.9f)
            {
                UpdateProgressBarValue(loadSceneRequest.progress / 0.9f);
                yield return ProgressBarValue;
            }

            ProgressBarToggle.Toggle.TurnOff();
            yield return new WaitForSeconds(ProgressBarToggle.TransitionFadeOutLength);

            loadSceneRequest.allowSceneActivation = true;
        }
    }

    [Serializable]
    public class ToggleWrapper
    {
        public LeanToggle Toggle;
        public float TransitionFadeInLength = 0, TransitionFadeOutLength = 0;
    }
}

/// <summary>
/// Originally made for support of easier management of sequenced behaviour during startup, and support of movable behaviour blocks upon application start
/// </summary>
public abstract class SequencedBehaviourState
{
    protected SplashScreenBehaviour ctx;
    protected ProgressBar ProgressBar;
    protected SplashScreenBehaviour.ToggleWrapper ProgressBarToggle;
    private Coroutine _stateTask;

    public SequencedBehaviourState(SplashScreenBehaviour invoker)
    {
        ctx = invoker;
        
        ProgressBar = ctx.progressBar;
        ProgressBarToggle = ctx.progressBarToggle;

        OnTaskComplited.AddListener(ProgressBarToggle.Toggle.TurnOff);
    }

    public void Start()
    {
        _stateTask = ctx.StartCoroutine(Routine());
    }
    public readonly UnityEvent OnTaskComplited = new();
    protected float ProgressBarValue { get; private set; }
    protected abstract IEnumerator Routine();

    protected void UpdateProgressBarValue(params float [] values)
    {
        if (values.Any(val => val > 1)) throw new ArgumentException();
        if (values.Length < 1) throw new ArgumentNullException(nameof(values));
        ProgressBarValue = values.Sum() / values.Length;
        ProgressBar.currentPercent = ProgressBarValue * 100;
    }
}
