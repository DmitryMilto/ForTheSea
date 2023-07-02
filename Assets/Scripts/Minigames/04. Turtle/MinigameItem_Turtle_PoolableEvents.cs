using DG.Tweening;
using System;
using System.Linq;
using UnityEngine;

public class MinigameItem_Turtle_PoolableEvents : MinigameItem_PoolableEvents
{
    public float HidingDuration => 1 / VisibilityToggle.OffTransitions.Entries.Max(transition => transition.Speed);
    public Tweener MoveTweener { get; set; }

    public override void OnSpawn()
    {
        base.OnSpawn();
        ResetPosition();
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        MoveTweener?.Kill();
    }

    private void OnDisable()
    {
        MoveTweener?.Kill();
    }

    private void ResetPosition()
    {
        if (transform.parent is not RectTransform parentRectTransform) throw new ArgumentOutOfRangeException(nameof(transform.parent.name));
        transform.position = parentRectTransform.TransformPoint(parentRectTransform.pivot + new Vector2(parentRectTransform.rect.xMax/* * transform.lossyScale.x*/, 0));
    }
}