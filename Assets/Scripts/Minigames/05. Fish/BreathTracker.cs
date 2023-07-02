using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CoolableCoral))]
public class BreathTracker : MonoBehaviour
{
    private CoolableCoral FillProgress => _progress ?? GetComponent<CoolableCoral>(); private CoolableCoral _progress;
    private MinigameManagerAddon_Stamina Stamina => _stamina ?? FindObjectOfType<MinigameManagerAddon_Stamina>(); [SerializeField, ShowIf("@_stamina == null")] private MinigameManagerAddon_Stamina _stamina;
    private MinigameManager_Fish MinigameManager => _manager ?? FindObjectOfType<MinigameManager_Fish>(); [SerializeField, ShowIf("@_manager == null")] private MinigameManager_Fish _manager;
   
    private void OnEnable() => StartCoroutine(TrackRoutine());
    private void OnDisable() => StopCoroutine(TrackRoutine());
    
    protected virtual IEnumerator TrackRoutine()
    {
        while (enabled)
        {
            FillProgress.IsChangeAllowed = MinigameManager.IsRunning && MinigameManager.AddonHealth.IsDead is false;
            FillProgress.IsIncreasingAllowed = Stamina.IsOnCooldownRestore is false;
            yield return new WaitForEndOfFrame();
        }
    }
}
