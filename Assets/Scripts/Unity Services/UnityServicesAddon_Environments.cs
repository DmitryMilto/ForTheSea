using Sirenix.OdinInspector;
using Unity.Services.Core.Environments;
using UnityEngine;

public class UnityServicesAddon_Environments : UnityServices_Addons
{
    [SerializeField, AssetSelector] private UnityServicesEnvironment environmentData;

    public override void Init()
    {
        UnityServicesController.InitOptions.SetEnvironmentName(environmentData.EnvironmentName);
    }
}
