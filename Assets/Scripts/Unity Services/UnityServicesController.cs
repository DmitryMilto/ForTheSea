using Sirenix.OdinInspector;
using System;
using System.Collections;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;

public class UnityServicesController : MonoBehaviour
{
    public static UnityServicesController Instance { get; private set; }

    public UnityServicesAddon_Environments AddonEnvironments => addon_Environments && addon_Environments.gameObject.activeSelf ? addon_Environments : null; 
    [SerializeField, FoldoutGroup("Addons")] private UnityServicesAddon_Environments addon_Environments;
    public UnityServicesAddon_Authentication AddonAuthentication => addon_Authentication && addon_Authentication.gameObject.activeSelf ? addon_Authentication : null; 
    [SerializeField, FoldoutGroup("Addons")] private UnityServicesAddon_Authentication addon_Authentication;

    public InitializationOptions InitOptions
    {
        get => initOptions ??= new InitializationOptions();
        private set => initOptions = value;
    } [SerializeField, HideInInspector] private InitializationOptions initOptions;

    [SerializeField, FoldoutGroup("Events")] public UnityEvent<USCEvents> OnInitialized = new(), OnError = new();

    #region Debug

    [ShowInInspector, DisplayAsString, PropertyOrder(1)] public ServicesInitializationState CurrentState => Application.isPlaying ? UnityServices.State : ServicesInitializationState.Uninitialized;

    #endregion

    private bool hasStartInitFinished = false;

    [InfoBox("Cannot initialize while not in Play mode", VisibleIf = "@!UnityEngine.Application.isPlaying")]
    [InfoBox("Initialization in progress", VisibleIf = "@CurrentState == Unity.Services.Core.ServicesInitializationState.Initializing")]
    [Button(ButtonSizes.Large, Icon = SdfIconType.BootstrapReboot), DisableIf("@CurrentState == Unity.Services.Core.ServicesInitializationState.Initializing || !UnityEngine.Application.isPlaying")]
    public void Restart()
    {
        if (!hasStartInitFinished || CurrentState is ServicesInitializationState.Initializing) return;
        StartCoroutine(Start());
    }
    private IEnumerator Start()
    {
        float initElapsedDuration = 0;
        float maxInitDuration = 20;
        yield return Initialize();
        while (CurrentState is ServicesInitializationState.Initializing && initElapsedDuration < maxInitDuration)
        {
            yield return new WaitForSecondsRealtime(0.1f);
            initElapsedDuration += 0.1f;
        }
        //yield return new WaitWhile(() => CurrentState is ServicesInitializationState.Initializing);
        yield return Report();
        hasStartInitFinished = true;

        IEnumerator Initialize()
        {
            try
            {
                Instance = this;

                AddonEnvironments?.Init();
                UnityServices.InitializeAsync(InitOptions);
                AddonAuthentication?.Init();
            }
            catch (Exception)
            {
                OnError.Invoke(new USCEvents("Unity Gaming Services failed to initialize", gameObject));
                yield break;
            }
        }
        IEnumerator Report()
        {
            switch (CurrentState)
            {
                case ServicesInitializationState.Initialized: OnInitialized.Invoke(
                    new USCEvents("Unity Gaming Services Initialized!", gameObject)); break;
                default: OnError.Invoke(
                    new USCEvents("Unity Gaming Services failed to initialize", gameObject)); break;
            }
            yield return null;
        }
    }
    void OnDisable() => Instance = null;

    /// <summary>
    /// UnityServicesCustomEvents serves for providing a set of data for internal systems of product, so it behaves according on information in provided data.
    /// </summary>
    [Serializable]
    public class USCEvents
    {
        public DateTime EventCreationTime { get; private set; } 
        public string Message { get; private set; }
        public GameObject Sender { get; private set; }

        public USCEvents(string message, GameObject sender = null)
        {
            EventCreationTime = DateTime.Now;
            Message = message;
            Sender = sender;
        }
    }
}
