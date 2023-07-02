using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class HoldableButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [ShowInInspector, ReadOnly] public bool IsPressed
    {
        get => isPressed;
        private set
        {
            var previousState = isPressed;
            isPressed = value;

            if (previousState != value) OnIsPressedChanged.Invoke(value);

            switch (value)
            {
                case true when !previousState: OnPointerDownEvent.Invoke(); break;
                case false when previousState: OnPointerUpEvent.Invoke(); break;
            }
        }
    } private bool isPressed = false;
    [FoldoutGroup("Events")] public UnityEvent<bool> OnIsPressedChanged = new();
    [FoldoutGroup("Events"), LabelText("OnPointerDown")] public UnityEvent OnPointerDownEvent = new();
    [FoldoutGroup("Events"), LabelText("OnPointerUp")] public UnityEvent OnPointerUpEvent = new();

    public void OnPointerUp(PointerEventData _) => IsPressed = false;
    public void OnPointerDown(PointerEventData _) => IsPressed = true;
    private void OnApplicationFocus(bool _) => IsPressed = false;
    private void OnApplicationPause(bool _) => IsPressed = false;
    private void OnDisable() => IsPressed = false;
}