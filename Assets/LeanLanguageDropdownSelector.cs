using Lean.Localization;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Selectable))]
public class LeanLanguageDropdownSelector : SerializedMonoBehaviour
{
    enum Modes { FromOptionLabel, FromTable }

    [SerializeField, HideIf(nameof(TMPDropdown)), HorizontalGroup("Dropdowns"), HideLabel] private Dropdown legacyDropdown;
    [SerializeField, HideIf(nameof(legacyDropdown)), HorizontalGroup("Dropdowns"), HideLabel] private TMP_Dropdown TMPDropdown;

    [SerializeField] private Modes mode = Modes.FromOptionLabel;

    [OdinSerialize, ShowIf(nameof(mode), Modes.FromTable)] private Dictionary<int, string> IntToStringPairs;

    public void SetCurrentLanguageFromOptionLabel(int index)
    {
        //if (_Dropdown.options.Count < index) throw new IndexOutOfRangeException();
        var lang = legacyDropdown != null ? legacyDropdown.options[index].text : TMPDropdown.options[index].text;
        LeanLocalization.Instances.ForEach(localization => localization.SetCurrentLanguage(lang));
    }

    public void SetCurrentLanguageFromTable(int index) => LeanLocalization.Instances.ForEach(localization => localization.SetCurrentLanguage(IntToStringPairs[index]));
}
