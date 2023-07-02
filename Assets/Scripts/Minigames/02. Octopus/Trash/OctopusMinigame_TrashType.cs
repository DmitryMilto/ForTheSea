using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "##_## Trash type - ", menuName = "ScriptableObjects/Minigames/02. Octopus", order = 1)]
public class OctopusMinigame_TrashType : ScriptableObject
{
    public Sprite Icon => _icon; [VerticalGroup("Basic group/Left"), HorizontalGroup("Basic group", 100), PreviewField(100, ObjectFieldAlignment.Left)][SerializeField, Required, HideLabel] private Sprite _icon;
    public string Type => _type; [VerticalGroup("Basic group/Right"), LabelWidth(-105)][ValueDropdown(nameof(TrashTypes), ExpandAllMenuItems = true), SerializeField, Required] private string _type = "";
    public float Points => _points; [VerticalGroup("Basic group/Right"), LabelWidth(-105)][SerializeField, Required, Min(0)] private float _points = 0;

    [ShowInInspector, FoldoutGroup("Trash categories (Static list)"), PropertyOrder(float.MaxValue), LabelText("Enum"), ValidateInput(nameof(ValidateListInput))] public static List<string> TrashTypes = new() { "Glass", "Paper", "Organic", "Plastic" };

    private static bool ValidateListInput(List<string> input) => !input.Any(string.IsNullOrEmpty);
}
