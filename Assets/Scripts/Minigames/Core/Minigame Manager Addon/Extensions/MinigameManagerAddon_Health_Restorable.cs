using DG.Tweening;
using Lean.Common;
using Lean.Gui;
using Michsky.UI.ModernUIPack;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class MinigameManagerAddon_Health_Restorable : MinigameManagerAddon_Health
{
    [ShowInInspector, ProgressBar(0, "_restorePointsThreshold", Segmented = true), PropertyOrder(-2)] public int _healthRestoreProgress { get; private set; } = 0;

    [SerializeField, FoldoutGroup("Settings"), Min(1), OnValueChanged(nameof(Refresh)), PropertyOrder(-1)] private int _restorePointsThreshold = 150;

    [SerializeField, FoldoutGroup("Visual"), PropertyOrder(-1), LabelText("Increase Animation duration"), SuffixLabel("sec", true)] private float _onIncrease_AnimationDuration = 0.25f;

    [SerializeField, FoldoutGroup("Components"), LabelText("Slider Manager")] private SliderManager _healthRestore_SliderManager;
    [SerializeField, FoldoutGroup("Components"), LabelText("Label")] private LeanFormatString _healthRestore_Label;
    [SerializeField, FoldoutGroup("Components"), LabelText("Visibility toggle")] private LeanToggle _healthRestore_VisibilityToggle;

    [FoldoutGroup("Events"), PropertyOrder(1)] public UnityEvent<int> HealthRestoreCounter_OnValueChanged;

    protected override void Awake()
    {
        base.Awake();
        OnDamage.AddListener(value =>
        {
            ResetValue();
            Refresh();
            if (CurrentHP - value > 0) _healthRestore_VisibilityToggle.TurnOn();
        });
        OnHeal.AddListener(_ => Refresh());
        OnDeath.AddListener(() =>
        {
            _healthRestore_VisibilityToggle.TurnOff();
        });
        HealthRestoreCounter_OnValueChanged.AddListener(value =>
        {
            _healthRestoreProgress = value;
            Refresh();
        });
    }

    public void Refresh()
    {
        _healthRestore_SliderManager.mainSlider.minValue = 0;
        _healthRestore_SliderManager.mainSlider.maxValue = _restorePointsThreshold;

        DOTween.To(() => _healthRestore_SliderManager.mainSlider.value, value => _healthRestore_SliderManager.mainSlider.value = value, _healthRestoreProgress, _onIncrease_AnimationDuration);

        _healthRestore_Label.SetString(_healthRestoreProgress, _restorePointsThreshold);

        if (CurrentHP == MaxHealth) _healthRestore_VisibilityToggle.TurnOff();
        else if (CurrentHP > MaxHealth) Debug.LogError($"[{nameof(MinigameManagerAddon_Health_Restorable)}:{nameof(Refresh)}()]: Invalid CurrentHP", this);
    }

    public void TryAdd(int value)
    {
        if (!_healthRestore_VisibilityToggle.On) return;
        Add(value);
    }

    private void Add(int value)
    {
        if(CurrentHP == MaxHealth) return;

        _healthRestore_VisibilityToggle.TurnOn();

        var heartsToHeal_unclamped = Mathf.FloorToInt((_healthRestoreProgress + value) / _restorePointsThreshold);
        var heartsToHeal = Mathf.Clamp(heartsToHeal_unclamped, 0, MaxHealth - CurrentHP);
        if (heartsToHeal >= 1) Heal(heartsToHeal);
        HealthRestoreCounter_OnValueChanged.Invoke((_healthRestoreProgress + value) - (heartsToHeal * _restorePointsThreshold));
    }
    public void ResetValue() => HealthRestoreCounter_OnValueChanged.Invoke(0);
}
