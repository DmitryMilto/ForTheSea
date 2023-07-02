using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class CoolableCoral: MinigameManagerAddon_Stamina
{
    [FoldoutGroup("Components"), SerializeField] private HoldableButton button;
    public bool IsButtonCurrentlyHeld => button.IsPressed && !IsOnCooldownRestore;

    protected override IEnumerator TimedRoutine()
    {
        while (enabled)
        {
            if (IsChangeAllowed)
            {
                float value;
                if (IsButtonCurrentlyHeld && IsIncreasingAllowed) value = IncreaseRate / 100;
                else if (IsDecreasingAllowed) value = -DecreaseRate / 100;
                else value = 0;
                Mathf.Clamp01(Value += value / FixedTimeStep);
            }
            yield return new WaitForFixedUpdate();
        }
    }
}