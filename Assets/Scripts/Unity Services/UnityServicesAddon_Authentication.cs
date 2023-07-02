using Sirenix.OdinInspector;
//using Unity.Services.Authentication;
//using Unity.Services.Core;

public class UnityServicesAddon_Authentication : UnityServices_Addons
{
    //[ShowInInspector, InfoBox("UnityServices are not initialized. It initializes only in playmode or while playing on target platform"), ShowIf("@AuthInstance == null")]
    //private IAuthenticationService AuthInstance
    //{
    //    get
    //    {
    //        // suppress error flood in inspector
    //        try { return AuthenticationService.Instance; }
    //        catch (ServicesInitializationException) { return null; }
    //    }
    //}

    //[ShowInInspector] public bool IsSignedIn => AuthInstance?.IsSignedIn ?? false;
    //[ShowInInspector] public bool IsAuthorized => AuthInstance?.IsAuthorized ?? false;
    //[ShowInInspector] public bool IsExpired => AuthInstance?.IsExpired ?? false;
    //[ShowInInspector] public bool SessionTokenExists => AuthInstance?.SessionTokenExists ?? false;
    //[ShowInInspector] public string AccessToken => AuthInstance?.AccessToken;
    //[ShowInInspector] public string PlayerId => AuthInstance?.PlayerId;
    //[ShowInInspector] public string Profile => AuthInstance?.Profile;
    //[ShowInInspector] public PlayerInfoWrapper PlayerInfo => AuthInstance is not null ? new PlayerInfoWrapper() : null;

    public override async void Init()
    {
        //await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    //[Serializable] public class PlayerInfoWrapper
    //{
    //    private IAuthenticationService AuthInstance
    //    {
    //        get
    //        {
    //            // suppress error flood in inspector
    //            try { return AuthenticationService.Instance; }
    //            catch (ServicesInitializationException) { return null; }
    //        }
    //    }
    //    private PlayerInfo Data => AuthInstance.PlayerInfo;

    //    [ShowInInspector] public string ID => Data.Id;
    //    [ShowInInspector] public string CreatedAt => Data.CreatedAt.HasValue ? Data.CreatedAt.ToString() : "Never";
    //    [ShowInInspector] public List<Identity> Identities => identities ??= Data.Identities; [SerializeField, HideInInspector] private List<Identity> identities;
    //}
}
