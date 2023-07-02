using Lean.Gui;
using Lean.Localization;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ApplicationSettings : MonoBehaviour
{
    [BoxGroup("Control Panel", CenterLabel = true), ShowInInspector, LabelText("Music"), PropertyRange(0,1)] private float _inspector_Music{
        get => _slider_Music.Slider.value;
        set => _slider_Music.Slider.value = value;
    }
    [BoxGroup("Control Panel", CenterLabel = true), ShowInInspector, LabelText("Sounds"), PropertyRange(0,1)] private float _inspector_Sounds
    {
        get => _slider_Sounds.Slider.value;
        set => _slider_Sounds.Slider.value = value;
    }

    [BoxGroup("Control Panel", CenterLabel = true), SerializeField, ValueDropdown(nameof(GetLanguages)), OnValueChanged("@SetLanguage(_inspector_Language.name)"), LabelText("Language")] private LeanLanguage _inspector_Language;

    [FoldoutGroup("Settings"), SerializeField] private string _saveKey = "General/Settings";
    [FoldoutGroup("Settings"), SerializeField, ReadOnly, LabelText("Save")] private Save instance;

    [FoldoutGroup("Components"), SerializeField, LabelText("Slider: Music")] private SliderKeyPair _slider_Music;
    [FoldoutGroup("Components"), SerializeField, LabelText("Slider: Sounds")] private SliderKeyPair _slider_Sounds;
    [FoldoutGroup("Components"), SerializeField, LabelText("Toggle: Language")] private LeanToggle _toggle_Language;
    [FoldoutGroup("Components/Assets"), SerializeField] private AudioMixer _audioMixer;
    private bool saveOnExit = true;
    private INotifiable Notificator => ToastNotifications.Instance;

    void Awake()
    {
        instance = ES3.KeyExists(_saveKey)
            ? ES3.Load<Save>(_saveKey)
            : new Save { Music = _inspector_Music, Sounds = _inspector_Sounds, Language = _inspector_Language.name };
    }
    void Start()
    {
        Reapply();
    }

    void OnDisable()
    {
        WriteChanges();
    }

    /// <summary>
    /// Hook this for save loading, for example, on Application Open
    /// </summary>
    public void SetSliderSilent_Music(float value) => _slider_Music.Slider.SetValueWithoutNotify(value);
    /// <summary>
    /// Hook this for save loading, for example, on Application Open
    /// </summary>
    public void SetSliderSilent_Sounds(float value) => _slider_Sounds.Slider.SetValueWithoutNotify(value);
    /// <summary>
    /// Hook this for state updates, for example, when you setting value from script
    /// </summary>
    public void SetSlider_Music(float value) => _slider_Music.Slider.value = value;
    /// <summary>
    /// Hook this for state updates, for example, when you setting value from script
    /// </summary>
    public void SetSlider_Sounds(float value) => _slider_Sounds.Slider.value = value;
    /// <summary>
    /// Hook this to slider
    /// </summary>
    public void Set_Music(float valueRaw)
    {
        _audioMixer.SetFloat(_slider_Music.AudioMixerKey, Mathf.Log10(Mathf.Clamp01(valueRaw > 0 ? valueRaw : 0.0001f)) * 20);
        instance.Music = valueRaw;
    }
    /// <summary>
    /// Hook this to slider
    /// </summary>
    public void Set_Sound(float valueRaw)
    {
        _audioMixer.SetFloat(_slider_Sounds.AudioMixerKey, Mathf.Log10(Mathf.Clamp01(valueRaw > 0 ? valueRaw : 0.0001f)) * 20);
        instance.Sounds = valueRaw;
    }
    public void SetLanguage(string value)
    {
        if (string.IsNullOrEmpty(value) is false)
        {
            LeanLocalization.SetCurrentLanguageAll(value);
            instance.Language = value;
        }
        else
        {
            Debug.LogError($"[{nameof(ApplicationSettings)}] SetLanguage: Incorrect argument ({value})");
            LeanLocalization.SetCurrentLanguageAll("English");
            instance.Language = "English";
        }
    }

    public void SetToggle_Language(string value) => _toggle_Language.Set(FindObjectOfType<LeanLocalization>().DefaultLanguage != value);

    private void Reapply(bool silent = false)
    {
        if (silent)
        {
            SetSliderSilent_Music(instance.Music);
            SetSliderSilent_Sounds(instance.Sounds);
        }
        else
        {
            SetSlider_Music(instance.Music);
            SetSlider_Sounds(instance.Sounds);
        }
        Set_Music(instance.Music);
        Set_Sound(instance.Sounds);
        SetLanguage(instance.Language);
        SetToggle_Language(instance.Language);
    }
    public void WriteChanges()
    {
        if (saveOnExit) ES3.Save(_saveKey, instance);
    }

    public void ClearSave()
    {
        saveOnExit = false;
        if (ES3.KeyExists(_saveKey))
        {
            ES3.DeleteKey(_saveKey);
            Notificator.Notify(new INotifiable.NotificationData(type: "Success", message: "Save cleared successfully!"));
        }
        else Notificator.Notify(new INotifiable.NotificationData(type: "Error", message: "Save doesn't exist"));
    }

    private List<LeanLanguage> GetLanguages() => LeanLocalization.CurrentLanguages.Values.ToList();
    [Serializable] private class SliderKeyPair
    {
        public Slider Slider => slider; [SerializeField, Required] private Slider slider;
        public string AudioMixerKey => audioMixerKey; [SerializeField, Required] private string audioMixerKey;
    }

    [Serializable] public class Save
    {
        [MinValue(0), MaxValue(1)] public float Music = 1;
        [MinValue(0), MaxValue(1)] public float Sounds = 1;
        public string Language = "English";
    }
}
