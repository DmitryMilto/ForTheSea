using Lean.Gui;
using Lean.Pool;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(RectTransform))]
public class NotificationLinker_Toast : MonoBehaviour, IPoolable/*, ICloneable*/
{
    /// <summary>
    /// Remaining time from Pulser
    /// </summary>
    [FoldoutGroup("Settings"), SerializeField, OnValueChanged(nameof(ResetController))] private float activateDelay = 0; 
    /// <summary>
    /// Time interval from Pulser
    /// </summary>
    [FoldoutGroup("Settings"), SerializeField, OnValueChanged(nameof(ResetController))] private float aliveDuration = 5;

    public TMP_Text Header => header; [FoldoutGroup("Components"), SerializeField, Required] private TMP_Text header;
    public TMP_Text Message => message; [FoldoutGroup("Components"), SerializeField, Required] private TMP_Text message;
    public Image Icon => icon; [FoldoutGroup("Components"), SerializeField, Required] private Image icon;
    public LeanPulse VisibilityController => visibilityController; [FoldoutGroup("Components"), SerializeField, Required] private LeanPulse visibilityController;
    public LeanToggle VisibilityToggle => visibilityToggle; [FoldoutGroup("Components"), SerializeField, Required] private LeanToggle visibilityToggle;
    public LeanButton CloseButton => closeButton; [FoldoutGroup("Components"), SerializeField, Required] private LeanButton closeButton;

    public void OnSpawn()
    {
        //обновляет при добавлении
        //LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

        //(transform as RectTransform).ForceUpdateRectTransforms();
        //ToastNotifications.Instance.Container?.GetComponent<HorizontalOrVerticalLayoutGroup>().CalculateLayoutInputVertical();
        visibilityController.enabled = true;
    }

    public void Despawn(float delay = 0)
    {
        LeanPool.Despawn(this, delay);
    }
    public void OnDespawn()
    {
        ResetController();
    }
    public void TryClose()
    {
        if (CloseButton.isActiveAndEnabled)
        {
            ResetController();
            CloseButton.OnClick.Invoke();
        }
    }

    private void ResetController()
    {
        visibilityController.enabled = false;
        visibilityController.RemainingPulses = 2;
        VisibilityController.RemainingTime = activateDelay;
        visibilityController.TimeInterval = aliveDuration;
    }
}