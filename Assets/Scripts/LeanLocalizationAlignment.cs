using Lean.Localization;
using TMPro;
using UnityEngine;

public class LeanLocalizationAlignment : MonoBehaviour
{
    [SerializeField] private TextAlignmentOptions options = TextAlignmentOptions.Right;
    private TMP_Text mP_Text;
    string local;
    private string Local
    {
        get
        {
            return local = LeanLocalization.GetFirstCurrentLanguage();
        }
    }
    private TMP_Text m_Text
    {
        get 
        { 
            if(mP_Text== null)
                mP_Text= GetComponent<TextMeshProUGUI>();
            return mP_Text; 
        }
    }

    private void Start()
    {
        LeanLocalization.OnLocalizationChanged += UpdateLocalization;
    }
    private void OnDisable()
    {
        LeanLocalization.OnLocalizationChanged -= UpdateLocalization;
    }
    private void UpdateLocalization()
    {
        if (Local == "Arabic")
        {
            m_Text.alignment = options;
        }
        else
        {
            m_Text.alignment = TextAlignmentOptions.Center;
        }
    }
}
