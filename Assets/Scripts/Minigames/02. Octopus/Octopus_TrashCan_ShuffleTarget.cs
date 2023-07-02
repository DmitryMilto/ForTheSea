using Lean.Gui;
using Sirenix.OdinInspector;
using UnityEngine;

public class Octopus_TrashCan_ShuffleTarget : MonoBehaviour, IOctopusShuffable
{
    #region IOctopusShuffable
    [ShowInInspector] public RectTransform Target => VisibilityToggle.transform as RectTransform;
    public float PreShuffleDelay => visibilityDelayIsDifferent ? visibilityDelay_OnOn : visibilityDelay;
    public float PostShuffleDelay => visibilityDelayIsDifferent ? visibilityDelay_OnOff : visibilityDelay;

    #endregion

    #region Visibility

    public LeanToggle VisibilityToggle => visibilityToggle; [SerializeField] private LeanToggle visibilityToggle;

    [SerializeField, FoldoutGroup("Visibility")] private bool visibilityDelayIsDifferent = false;
    [SerializeField, FoldoutGroup("Visibility"), HideIf(nameof(visibilityDelayIsDifferent)), SuffixLabel("sec", true)] private float visibilityDelay = 1;
    [SerializeField, FoldoutGroup("Visibility"), ShowIf(nameof(visibilityDelayIsDifferent)), SuffixLabel("sec", true)] private float visibilityDelay_OnOn = 1;
    [SerializeField, FoldoutGroup("Visibility"), ShowIf(nameof(visibilityDelayIsDifferent)), SuffixLabel("sec", true)] private float visibilityDelay_OnOff = 1;

    #endregion

    [SerializeField] internal LeanShake shaker;
    public Octopus_DragableItemTarget TrashCan => trashcan ??= shaker.GetComponent<Octopus_DragableItemTarget>(); [SerializeField] private Octopus_DragableItemTarget trashcan;

    public void OnStartShuffle()
    {
        VisibilityToggle.TurnOff();
        shaker.enabled = false;
    }

    public void OnEndShuffle()
    {
        Target.localRotation = Quaternion.Euler(Vector3.zero);
        Target.anchoredPosition = Vector2.zero;
        shaker.enabled = true;
        VisibilityToggle.TurnOn();
    }
}