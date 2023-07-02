using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tutorial SaveData", menuName = "ScriptableObjects/Tutorials/New Tutorial SaveData", order = 1)]
public class TutorialSaveData : ScriptableObject
{
    [SerializeField] private Modes showConditions = Modes.OnceInSession;
    public string SaveKey => saveKey; [SerializeField] private string saveKey = "Tutorials/Default game";

    /// <summary>
    /// This field will be reset by mechanic of scriptable objects as then persist data when application running, but reset to initial state or restart of it.
    /// As for editor, when playmode exits - mechanic designed that way, that when application closes - it sets state to 'false', meaning initial state.
    /// </summary>
    [SerializeField] public bool TutorialShownInThisSession = false;

    public bool ShouldTutorialBeShown(SaveDataContainer save) =>
        showConditions switch
        {
            Modes.EveryTime => true,
            Modes.OnceInSession => !TutorialShownInThisSession,
            Modes.Once => !save.TutorialShown,
            Modes.Never => false,
            _ => throw new ArgumentOutOfRangeException()
        };

    public enum Modes
    {
        EveryTime,
        OnceInSession,
        Once,
        Never
    }
    [Serializable] public class SaveDataContainer
    {
        public bool TutorialShown
        {
            get => tutorialShown;
            set => tutorialShown = value;
        } [SerializeField] private bool tutorialShown = false;
    }
}
