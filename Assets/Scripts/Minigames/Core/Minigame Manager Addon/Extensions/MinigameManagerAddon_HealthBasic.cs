using Sirenix.OdinInspector;
using UnityEngine.Events;

public class MinigameManagerAddon_HealthBasic : MinigameManagerAddon_Core
{
    [FoldoutGroup("Events"), PropertyOrder(1)] public UnityEvent OnDeath;
    public bool IsDead { get; private set; } = false;
    public virtual void Kill() => OnDeath.Invoke();

    void OnEnable()
    {
        OnDeath?.AddListener(() => IsDead = true);
    }
}
