using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CircleCollider2D))]
public class CutSwiper : MonoBehaviour
{
    [ShowInInspector, ReadOnly] public Vector2 PointerPosition { get; private set; }
    [ShowInInspector, ReadOnly] public Vector2 Velocity => previousPosition == Vector2.zero ? Vector3.zero : PointerPosition - previousPosition;

    [Header("Visual")] 
    [SerializeField] private Transform swipeTrail;
    [SerializeField] private string pathToTrailVisual;
    //[SerializeField] private CanvasScaler globalCanvas;

    [Header("Behaviour")] 
    public UnityEvent<bool> OnTouch = new();

    private TrailRenderer _trailRenderer;
    private TrailRenderer TrailRenderer
    {
        get
        {
            if (_trailRenderer is not null) return _trailRenderer;

            // Attempt to get trail by path. if failed - try searching by type. If none found - throw error.
            return _trailRenderer = GetByPath() ?? GetByType() ?? throw new MissingComponentException();;

            TrailRenderer GetByPath()
            {
                var trailGameObjectCandidate = swipeTrail.Find(pathToTrailVisual);
                return trailGameObjectCandidate is not null ? GetTrail(trailGameObjectCandidate) : null;
            }
            TrailRenderer GetByType() => GetTrail(swipeTrail);
            TrailRenderer GetTrail(Transform source)
            {
                var found = source.TryGetComponent(out TrailRenderer trail);
                return found ? trail : null;
            }
        }
        set => _trailRenderer = value ?? throw new ArgumentException(nameof(value));
    }
    private Camera mainCamera;
    private Camera MainCamera
    {
        get => mainCamera ??= Camera.main is not null ? Camera.main : throw new NullReferenceException();
        set => mainCamera = value ?? throw new NullReferenceException();
    }

    private Vector2 previousPosition = Vector2.zero;

    private bool ApplicationFocused { get; set; }
    
    private void UpdatePointerPosition(Vector2 value)
    {
        previousPosition = PointerPosition;
        swipeTrail.position = PointerPosition = MainCamera.ScreenToWorldPoint(value);
    }

    private void UpdateTrailVisibility(bool value)
    {
        if (TrailRenderer.emitting == value) return;
        var wasEnabled = TrailRenderer.emitting;
        TrailRenderer.emitting = value;
        // preventing of connecting touching points between different taps
        if (wasEnabled is false && value is true) TrailRenderer.Clear();

        if (value is false) previousPosition = Vector2.zero;

        OnTouch.Invoke(value);
    }

    #region InputCaptureModes is StaticInputClasses

    void Update()
    {
        if (ApplicationFocused is false) return; 
        UpdatePointerPosition(Pointer.current.position.ReadValue());
        UpdateTrailVisibility(Pointer.current.press.isPressed);
    }
    
    #endregion

    private void OnApplicationFocus(bool value) => ApplicationFocused = value;
}
