using Lean.Gui;
using Michsky.UI.ModernUIPack;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMenuLoader : MonoBehaviour
{
    [SerializeField, Required, FoldoutGroup("Components")]
    private LeanToggle blackScreen;
    [SerializeField, Required, FoldoutGroup("Components/Black screen"), ShowIf("@blackScreen != null"), Min(0)]
    private float blackScreen_fadeInDelay, blackScreen_fadeOutDelay;

    [SerializeField, Required, FoldoutGroup("Progress bar")]
    private LeanToggle progressBarVisibility;
    [SerializeField, Required, FoldoutGroup("Progress bar")]
    private ProgressBar progressBar;

    [SerializeField, Min(0), FoldoutGroup("Settings")]
    private float delayAfterLoad;

    [SerializeField, Required, FoldoutGroup("Scene settings")] 
    private SceneReference sceneToLoad;
    [SerializeField, FoldoutGroup("Scene settings")] 
    private LoadSceneMode loadSceneMode = LoadSceneMode.Single;
    [SerializeField, FoldoutGroup("Scene settings")]
    private bool allowSceneActivationByDefault = false;
    [ShowInInspector, FoldoutGroup("Debug")] public bool loaderExists => loader is not null;
    [ShowInInspector, FoldoutGroup("Debug"), HideIf("@loaderExists == false")] public bool loadInProgress => loaderExists && loader.isDone is false;
    [ShowInInspector, FoldoutGroup("Debug"), HideIf("@loadInProgress == false")] public float loadProgress => loadInProgress ? loader.progress : 0;

    private AsyncOperation loader;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(blackScreen_fadeInDelay);

        yield return RequestLoad();
        loader.allowSceneActivation = allowSceneActivationByDefault;
        progressBarVisibility.TurnOn();

        yield return WaitForLoad();

        progressBarVisibility.TurnOff();

        yield return new WaitForSeconds(delayAfterLoad);

        blackScreen.TurnOn();

        yield return new WaitForSeconds(blackScreen_fadeOutDelay);
        loader.allowSceneActivation = true;
    }

    private IEnumerator RequestLoad()
    {
        var sceneName = sceneToLoad.ScenePath.Split(new []{"/", ".unity"}, StringSplitOptions.RemoveEmptyEntries)[^1];
        loader = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
        yield return null;
    }

    private IEnumerator WaitForLoad()
    {
        while (loader.isDone)
        {
            if (loader.progress >= 0.9f && loader.allowSceneActivation is false) break;
            progressBar.ChangeValue(loader.progress * 100);
            yield return null;
        }
    }
}
