using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Controller_SwipeCutter : MonoBehaviour
{
    public float MinSwipeSpeed => minSwipeSpeed; [FoldoutGroup("Settings")] private float minSwipeSpeed = 1;
    [FoldoutGroup("Settings"), SerializeField] private ContactFilter2D RaycastMask;

    private Camera Camera => _camera ??= Camera.main; [FoldoutGroup("Components"), SerializeField] private Camera _camera;

    [FoldoutGroup("Components/Prefabs"), SerializeField, AssetsOnly] private GameObject trailPrefab;

    //[FoldoutGroup("Events")] public UnityEvent<Collider2D> OnTrigger;
    [FoldoutGroup("Events")] public UnityEvent<GameObject> OnCut;

    [ShowInInspector, ReadOnly] public Vector3 PointerPosition { get; private set; } = Vector3.zero;
    private Vector3 lastFramePointerPosition = Vector3.zero;

    [ShowInInspector,ReadOnly] public bool IsVisible { get; private set; }
    private bool wasVisibleLastFrame = false;

    [ShowInInspector, ReadOnly] public Vector3 Distance { get; private set; }
    [ShowInInspector, ReadOnly] public float CurrentSwipeSpeed => IsVisible ? Distance.magnitude : 0f;
    [ShowInInspector, ReadOnly] public Vector3 Direction => IsVisible ? Distance.normalized : Vector3.zero;

    private bool isApplicationFocused = true;
    private Collider2D[] currentCollisions = Array.Empty<Collider2D>(), previousCollisions = Array.Empty<Collider2D>();

    private void Update()
    {
        UpdatePointerPosition();
        UpdateVisuals();
        
        bool hasCollisions = false;
        Array.Clear(currentCollisions, 0, currentCollisions.Length);

        //Move this GameObject after Pointer
        if (IsVisible) transform.position = PointerPosition;

        // Abort execution if speed is not high enough
        if (CurrentSwipeSpeed < MinSwipeSpeed) return;

        if (isApplicationFocused) hasCollisions = TryRaycastPositions(ref currentCollisions);
        
        if (hasCollisions) currentCollisions.Except(previousCollisions).ToList().ForEach(OnCutTriggered);

        previousCollisions = currentCollisions.Intersect(previousCollisions).Union(currentCollisions).ToArray();

        void UpdatePointerPosition()
        {
            Vector3 newPosition = isApplicationFocused ? Pointer.current.position.ReadValue() : lastFramePointerPosition;
            lastFramePointerPosition = PointerPosition;
            PointerPosition = Camera.ScreenToWorldPoint(newPosition);

            Distance = PointerPosition - lastFramePointerPosition;
        }
        void UpdateVisuals()
        {
            wasVisibleLastFrame = IsVisible;
            IsVisible = isApplicationFocused && Pointer.current.press.isPressed;

            if (IsVisible && !wasVisibleLastFrame) Instantiate(trailPrefab, transform);
        }
        bool TryRaycastPositions(ref Collider2D[] hits)
        {
            if (wasVisibleLastFrame is false || IsVisible is false)
            {
                Array.Clear(hits, 0, hits.Length);
                return false;
            }
            List<RaycastHit2D> candidates = new();
            Physics2D.Raycast(lastFramePointerPosition, Direction, RaycastMask, candidates, CurrentSwipeSpeed);
            hits = candidates.ConvertAll(hit => hit.collider).ToArray();
            return hits.Any();
        }
    }

    private void OnCutTriggered(Collider2D data)
    {
        var linker = data.transform.parent.GetComponent<CrabMinigame_ItemLinker>() ?? data.GetComponent<CrabMinigame_ItemLinker>();
        linker ??= data.GetComponentInChildren<CrabMinigame_ItemLinker>();

        if (linker is null) throw new MissingComponentException();

        linker.OnSwipe.Invoke();
        OnCut.Invoke(linker.gameObject);
    }

    private void OnApplicationFocus(bool hasFocus) => isApplicationFocused = hasFocus;
}