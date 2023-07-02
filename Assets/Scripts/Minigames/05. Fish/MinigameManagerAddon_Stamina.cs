using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinigameManagerAddon_Stamina : MinigameManagerAddon_Core
{
    [BoxGroup("Settings"), ShowInInspector, PropertyRange(0,1)] private float RestoreThreshold { get; set; } = 0.5f;
    [ToggleGroup(nameof(IsChangeAllowed))] public bool IsChangeAllowed = true;
    [ToggleGroup(nameof(IsChangeAllowed)), EnableIf(nameof(IsChangeAllowed))] public bool IsIncreasingAllowed = true;
    [ToggleGroup(nameof(IsChangeAllowed)), EnableIf(nameof(IsIncreasingAllowed))] public float IncreaseRate = 10;
    [ToggleGroup(nameof(IsChangeAllowed)), EnableIf(nameof(IsChangeAllowed))] public bool IsDecreasingAllowed = true;
    [ToggleGroup(nameof(IsChangeAllowed)), EnableIf(nameof(IsDecreasingAllowed))] public float DecreaseRate = 1f;
  
    [FoldoutGroup("Components"), SerializeField] private Image fill;
    [FoldoutGroup("Components"), SerializeField] private TMP_Text fillPercentage;

    [ShowInInspector, ReadOnly] public virtual bool IsTriggered { get; set; }
    [ShowInInspector, ReadOnly] public bool IsOnCooldownRestore { get; private set; }
    [ShowInInspector, ReadOnly] public float Value
    {
        get => fill.fillAmount;
        set
        {
            value = Mathf.Clamp01(value);
            fill.fillAmount = value;
            fillPercentage?.SetText(Value.ToString("00.00%"));
        }
    }

    protected static float FixedTimeStep => 1 / Time.fixedDeltaTime;


    protected virtual void OnEnable() => StartCoroutine(TimedRoutine());
    protected virtual void OnDisable() => StopCoroutine(TimedRoutine());
    protected virtual IEnumerator TimedRoutine()
    {
        while (enabled)
        {
            if (IsChangeAllowed) Mathf.Clamp01(Value += (
                IsTriggered && !IsOnCooldownRestore ? 
                    (IsDecreasingAllowed ? (-DecreaseRate / 100) : 0) :
                    (IsIncreasingAllowed ? (IncreaseRate / 100) : 0)
                ) / FixedTimeStep);
            
            switch (IsOnCooldownRestore)
            {
                case false when Value == 0: IsOnCooldownRestore = true; IsTriggered = false; break;
                case true when Value >= RestoreThreshold: IsOnCooldownRestore = false; break;
            }

            yield return new WaitForFixedUpdate();
        }
    }
}
