using Lean.Gui;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LeanDrag))]
public class Octopus_DragableItem : MonoBehaviour
{
    [ShowInInspector, AssetList(AutoPopulate = true, Path = @"Scripts\Minigames\02. Octopus\Trash"), ReadOnly, PropertyOrder(-1)] public OctopusMinigame_TrashType CurrentlyHeldItem
    {
        get => itemVisibilityToggle.On ? currentlyHeldItem : null;
        private set
        {
            if (value is not null)
            {
                iconComponent.sprite = value.Icon;
                iconComponent.color = new Color(1, 1, 1, 1);
            }
            else
            {
                iconComponent.sprite = defaultIconData.Sprite;
                iconComponent.color = defaultIconData.Color;
            }

            currentlyHeldItem = value;
        }
    } [SerializeField, HideInInspector, HorizontalGroup("Basic group"), VerticalGroup("Basic group/Left")] private OctopusMinigame_TrashType currentlyHeldItem;
    
    [FoldoutGroup("Components"), SerializeField, Required] private Image iconComponent;
    [FoldoutGroup("Components"), SerializeField, Required] private LeanToggle itemVisibilityToggle;
   
    [FoldoutGroup("Default Icon Data"), SerializeField, HideLabel] private DefaultIconData defaultIconData = new();

    private void Awake()
    {
        if (iconComponent is null) throw new MissingComponentException();
        defaultIconData.CopyFrom(iconComponent);
    }

    [HorizontalGroup("Spawn", 0.5f), BoxGroup("Spawn/Left", false), Button(ButtonStyle.Box, Expanded = true)]
    public void OnSpawn(OctopusMinigame_TrashType value)
    {
        CurrentlyHeldItem = value;
        itemVisibilityToggle.TurnOn();
    }
    [HorizontalGroup("Spawn", 0.5f), BoxGroup("Spawn/Right", false), Button(ButtonStyle.Box, ButtonHeight = 78)]
    public void OnDespawn()
    { 
        itemVisibilityToggle.TurnOff();
    }

    [HorizontalGroup("Basic group", 100), VerticalGroup("Basic group/Right", VisibleIf = "@CurrentlyHeldItem != null"), Button(Name = "Reset", ButtonHeight = 45)] 
    public void ResetType()
    {
        CurrentlyHeldItem = null;
    }

    [Serializable]
    private class DefaultIconData : IconData
    {
        public override Sprite Sprite
        {
            get => sprite;
            internal set => sprite = value;
        } [SerializeField, EnableIf(nameof(InEditor))] private Sprite sprite;

        public override Color Color
        {
            get => color;
            internal set => color = value;
        } [SerializeField, EnableIf(nameof(InEditor))] private Color color;

        [Button, ShowIf(nameof(InEditor))]
        internal void CopyFrom(Image data) => Set(data);
        private bool InEditor => Application.isEditor;
    }

    [Serializable] public abstract class IconData
    {
        public abstract Sprite Sprite { get; internal set; }

        public abstract Color Color { get; internal set; }

        private protected void Set(Image value) => Set(value.sprite, value.color);

        private protected void Set(Sprite sprite, Color color)
        {
            Sprite = sprite;
            Color = color;
        }
    }

}
