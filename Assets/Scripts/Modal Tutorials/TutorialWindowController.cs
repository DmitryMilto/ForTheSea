using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TutorialWindowController : MonoBehaviour
{
    public bool IsSkippable => isSkippable; [SerializeField] private bool isSkippable = true;
    [SerializeField] private RectTransform stepsContainer;
    [ShowInInspector, HorizontalGroup(GroupName = "Steps"), InlineButton(nameof(RefillSteps), SdfIconType.Search, "")] public LinkedList<TutorialWindow> Steps
    {
        get => new(_steps);
        set => _steps = value.ToList();
    }
    [SerializeField, HideInInspector] private List<TutorialWindow> _steps;
    [SerializeField, FoldoutGroup("Events")] public UnityEvent OnTutorialStart, OnTutorialEnd;

    private LinkedListNode<TutorialWindow> GetCurrent(TutorialWindow tutorialWindow) => Steps.Find(tutorialWindow) ?? throw new ArgumentOutOfRangeException();
    public bool IsFirst(TutorialWindow requester) => Steps.First.Value == requester;
    public bool IsLast(TutorialWindow requester) => Steps.Last.Value == requester;
    public void ShowTutorial()
    {
        OnTutorialStart.Invoke();
        Steps.First.Value.Window.TurnOn();
    }
    public void ShowNext(TutorialWindow requester)
    {
        var current = GetCurrent(requester);
        var next = current.Next;
        if (next is not null)
        {
            current.Value.PulseDisappearLeft.BeginTransitions();
            next.Value.Window.TurnOn();
            next.Value.PulseAppearRight.BeginTransitions();
        }
        else Debug.Log("List end.");
    }
    public void ShowPrevious(TutorialWindow requester)
    {
        var current = GetCurrent(requester);
        var previous = current.Previous;
        if (previous is not null)
        {
            current.Value.PulseDisappearRight.BeginTransitions();
            previous.Value.Window.TurnOn();
            previous.Value.PulseAppearLeft.BeginTransitions();
        }
        else Debug.Log("List end.");
    }
    public void SkipTutorial()
    {
        Steps.Where(window => window.Window.On).ToList().ForEach(window => window.Window.TurnOff());
        OnTutorialEnd.Invoke();
    }

    private void RefillSteps() => Steps = new LinkedList<TutorialWindow>(stepsContainer != null ? stepsContainer.GetComponentsInChildren<TutorialWindow>() : GetComponentsInChildren<TutorialWindow>());
}
