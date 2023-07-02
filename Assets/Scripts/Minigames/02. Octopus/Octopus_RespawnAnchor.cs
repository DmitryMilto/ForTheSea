using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public class Octopus_RespawnAnchor : MonoBehaviour
{
    [FoldoutGroup("Components"), ShowInInspector, ShowIf("@Content != null"), ReadOnly] private RectTransform Content => _content ??= transform.GetComponentsInChildren<RectTransform>().FirstOrDefault(child => child.name == "Content"); [SerializeField, ShowIf("@_content == null")] private RectTransform _content;
}
