using Lean.Gui;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class MinigameItem_PoolableEvents : MonoBehaviour, IPoolable
{
    public LeanToggle VisibilityToggle => visibilityToggle; [SerializeField] private LeanToggle visibilityToggle;
    [FoldoutGroup("Events")] public UnityEvent OnSpawnActions, OnDespawnActions, OnTriggerActions, OnNoTriggerActions;
    public bool WasTriggered { get; private set; }

    public virtual void OnTriggerDetected()
    {
        WasTriggered = true;
        OnTriggerActions.Invoke();
    }

    public virtual void OnSpawn() { }

    public virtual void Init() => OnSpawnActions.Invoke();

    public virtual void OnDespawn()
    {
        OnSpawnActions.RemoveAllListeners();
        OnDespawnActions.RemoveAllListeners();
        OnTriggerActions.RemoveAllListeners();
        OnNoTriggerActions.RemoveAllListeners();
        WasTriggered = false;
    }
}
// To spawn:
// 1. var obj = LeanPool.Instantiate(GameObject);
// 2. var objLinker = obj.GetComponent<MinigameItem_Turtle_PoolableEvents>();
// 3. objLinker.OnSpawnActions.AddListener(() => {/*...*/});
// 4. objLinker.OnDespawnActions.AddListener(() => {/*...*/});
// 5. objLinker.OnTriggerActions.AddListener(() => {/*...*/});
// 5. objLinker.OnNoTriggerActions.AddListener(() => {/*...*/});
// 6. objLinker.Init();

// To despawn:
// 1. OnDespawnActions.Invoke();

// If purpose reached:
// 1. OnTriggerActions.Invoke();
// Note: purpose defines behaviour. By default, despawn-ing with score increase recommended

