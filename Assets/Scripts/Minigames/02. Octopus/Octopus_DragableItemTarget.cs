using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Octopus_DragableItemTarget : MonoBehaviour, IDropHandler
{
    public string LookForType => _lookForType; [ValueDropdown(nameof(TrashTypes), ExpandAllMenuItems = true), SerializeField, Required] private string _lookForType = "";
    [SerializeField, ReadOnly] private DragTarget_TrashInfo _trashInfo;
    [FoldoutGroup("Events")] public UnityEvent OnMatch, OnMismatch;

    private List<string> TrashTypes => OctopusMinigame_TrashType.TrashTypes;

    public void OnDrop(PointerEventData eventData)
    {
        _trashInfo = new() { DroppedItem = eventData.pointerDrag.transform };

        bool isMatching = _trashInfo.Compare(LookForType);

        if (isMatching) OnMatch?.Invoke();
        else OnMismatch?.Invoke();

        _trashInfo.Item.OnDespawn();
    }

    [Serializable] public class DragTarget_TrashInfo
    {
        public Transform DroppedItem { 
            get => droppedItem;
            set
            {
                if (value is null) return;
                droppedItem = value;
                InvokeSource = value.parent.parent;
                value.TryGetComponent(out Octopus_DragableItem item);
                Item = item;
                ItemType = item.CurrentlyHeldItem;
            }
        } [SerializeField, ReadOnly] private Transform droppedItem;
        public Transform InvokeSource;
        public Octopus_DragableItem Item;
        public OctopusMinigame_TrashType ItemType;

        internal void Log(bool isMatching) => Debug.Log($"Source = {InvokeSource.name}; Dropped item = {(ItemType ? $"[{ItemType.Type}] {ItemType.Icon.name}" : DroppedItem.name)}, Is Matching type = {isMatching}", DroppedItem);
        public bool Compare(string LookForType) => ItemType && ItemType.Type == LookForType;
    }
}
