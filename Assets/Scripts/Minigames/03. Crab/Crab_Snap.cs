using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Responsible for playing animation, that depends on side of the screen
/// </summary>
[RequireComponent(typeof(Animator))]
public class Crab_Snap : MonoBehaviour
{
    private Animator animator => _animator ??= GetComponent<Animator>(); [SerializeField] private Animator _animator;

    public void OnSnap() => animator.SetTrigger(Pointer.current.position.ReadValue().x > Screen.width / 2 ? "Snap: Right" : "Snap: Left");
}
