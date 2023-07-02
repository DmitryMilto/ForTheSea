using UnityEngine;

public abstract class UnityServices_Addons : MonoBehaviour
{
    public UnityServicesController UnityServicesController => UnityServicesController.Instance;
    public abstract void Init();
}
