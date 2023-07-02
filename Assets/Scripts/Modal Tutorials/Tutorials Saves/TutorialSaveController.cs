using Lean.Gui;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(TutorialWindowController), typeof(LeanPulse))]
public class TutorialSaveController : MonoBehaviour
{
    #region Self-initializable fields

    public TutorialWindowController Controller
    {
        get => controller ??= GetComponent<TutorialWindowController>();
        set => controller = value;
    } [SerializeField, HideInInspector] private TutorialWindowController controller;
    public LeanPulse Pulser
    {
        get => pulser ??= GetComponent<LeanPulse>();
        set => pulser = value;
    } [SerializeField, HideInInspector] private LeanPulse pulser;

    #endregion

    [SerializeField, InlineEditor(InlineEditorObjectFieldModes.Foldout, Expanded = true)] private TutorialSaveData settings;
    [SerializeField, ReadOnly, /*ShowIf(nameof(save)),*/ ShowIf(nameof(settings))] private TutorialSaveData.SaveDataContainer save;

    private void Awake()
    {
        LoadSave();
    }

    private void Start()
    {
        DecideShowingTutorial();

        void DecideShowingTutorial()
        {
            if (settings.ShouldTutorialBeShown(save ??= new TutorialSaveData.SaveDataContainer()))
            {
                save.TutorialShown = settings.TutorialShownInThisSession = true;
                Pulser.RemainingPulses = 1;
            }
            else
            {
                Pulser.RemainingPulses = 0;
                Controller.OnTutorialEnd.Invoke();
            }
        }
    }

    private void OnApplicationPause(bool isPaused)
    {
        if(isPaused) WriteSave();
    }

    private void OnDestroy()
    {
         WriteSave();
    }

    private void OnApplicationQuit()
    {
        settings.TutorialShownInThisSession = false;
    }

    #region Save

    [TitleGroup("Save")]
    [ButtonGroup("Save/Operations"), EnableIf(nameof(IsInPlaymode))] private void LoadSave()
    {
        Debug.Log($"[{GetType().Name}] Save = \"{JsonUtility.ToJson(save)}\"");
        if (ES3.KeyExists(settings.SaveKey)) save = ES3.Load<TutorialSaveData.SaveDataContainer>(settings.SaveKey);
    }
    [ButtonGroup("Save/Operations"), EnableIf(nameof(IsInPlaymode))] private void WriteSave()
    {
        Debug.Log($"[{GetType().Name}] Writing \"{JsonUtility.ToJson(save)}\" to save");
        ES3.Save(settings.SaveKey, save);
    }
    [ButtonGroup("Save/Operations"), EnableIf(nameof(SaveExists))] private void ResetSave()
    {
        if (SaveExists)
        {
            Debug.Log("ResetSave: Success");
            ES3.DeleteKey(settings.SaveKey);
        }
        else
        {
            Debug.Log("ResetSave: No save for key");
        }
    }

    private static bool IsInPlaymode => Application.isPlaying;
    public bool SaveExists => ES3.KeyExists(settings.SaveKey);

    #endregion
}
