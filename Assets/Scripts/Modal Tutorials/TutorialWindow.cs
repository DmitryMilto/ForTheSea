using Lean.Gui;
using Lean.Transition;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LeanWindow))]
public class TutorialWindow : MonoBehaviour
{
    public TutorialWindowController Controller => controller ??= TryGetController(); [SerializeField, InlineButton(nameof(FindAndSetController), SdfIconType.Search, ""),
                                                                   ValidateInput(nameof(IsChildOfController), "Warning: Not child of \"TutorialWindowController\"", InfoMessageType.Warning), 
                                                                   DisableIf("@IsChildOfController() && controller != null")] private TutorialWindowController controller;
    public LeanWindow Window => window; [SerializeField] private LeanWindow window;

    #region Components

    public Selectable NextSlide => nextSlide; [SerializeField] private Selectable nextSlide;
    public Selectable PreviousSlide => previousSlide; [SerializeField] private Selectable previousSlide;
    public Selectable Close => closeTutorial; [SerializeField] private Selectable closeTutorial;
    public Selectable Skip => skipTutorial; [SerializeField] private Selectable skipTutorial;

    #endregion
    #region Pulse

    public LeanManualAnimation PulseAppearLeft => pulseAppearLeft; [SerializeField, FoldoutGroup("Pulse")] private LeanManualAnimation pulseAppearLeft;
    public LeanManualAnimation PulseAppearRight => pulseAppearRight; [SerializeField, FoldoutGroup("Pulse")] private LeanManualAnimation pulseAppearRight;
    public LeanManualAnimation PulseDisappearLeft => pulseDisappearLeft; [SerializeField, FoldoutGroup("Pulse")] private LeanManualAnimation pulseDisappearLeft;
    public LeanManualAnimation PulseDisappearRight => pulseDisappearRight; [SerializeField, FoldoutGroup("Pulse")] private LeanManualAnimation pulseDisappearRight;

    #endregion

    private void Start()
    {
        Window.OnOn.AddListener(OnShow);
    }

    public void ShowNext() => Controller.ShowNext(this);
    public void ShowPrevious() => Controller.ShowPrevious(this);
    public void SkipTutorial() => Controller.SkipTutorial();

    public void OnShow()
    {
        var isLast = Controller.IsLast(this);

        NextSlide.interactable = !isLast;
        PreviousSlide.interactable = Controller.IsFirst(this) is false;
        Skip.interactable = Controller.IsSkippable;
        
        // if slide is last - replace buttons
        Close.gameObject.SetActive(isLast);
        NextSlide.gameObject.SetActive(!isLast);
        Skip.gameObject.SetActive(!isLast);
    }

    #region Editor-only

    private bool IsChildOfController() => transform.parent.TryGetComponent(out TutorialWindowController _);
    private TutorialWindowController TryGetController() =>
        IsChildOfController()
            ? transform.parent.GetComponentInParent<TutorialWindowController>()
            : FindObjectOfType<TutorialWindowController>() ?? throw new NullReferenceException();

    private void FindAndSetController() => controller ??= TryGetController();

    #endregion
}
