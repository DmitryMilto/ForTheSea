using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Toasts Collection", menuName = "ScriptableObjects/Notification collection/Toast")]
public class NotificationLibrary_Toast : ScriptableObject
{
    public List<Item> Library => library; [SerializeField, TableList] private List<Item> library;

    [Serializable] public class Item
    {
        public GameObject Prefab => prefab; [SerializeField, AssetsOnly, ValidateInput("@prefab != null", "Prefab is required."), ValidateInput("@prefab == null || prefab.GetComponent<NotificationLinker_Toast>() != null", "Prefab doesn't have " + nameof(NotificationLinker_Toast) + " in root.")] private GameObject prefab;
        public NotificationLinker_Toast Linker => linker ??= Prefab?.GetComponent<NotificationLinker_Toast>(); [SerializeField, HideInInspector] private NotificationLinker_Toast linker;
        [ShowInInspector, DisplayAsString, HideIf("@Prefab == null"), TableColumnWidth(100, false)] public string Type => Prefab?.name.Split(". ").Last();
        [ShowInInspector, DisplayAsString, HideIf("@Prefab == null"), TableColumnWidth(100, false)] public bool HasLinker => Linker != null;
    }
}