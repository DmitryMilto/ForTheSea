using Lean.Gui;
using qASIC;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;
using static INotifiable;

public class PurchasingManager : MonoBehaviour, IStoreListener
{
    private static IStoreController controller;
    private static IExtensionProvider extensions;

    [TitleGroup("Load Scene")]
    [SerializeField] private LoadScene _loadScene;

    [TitleGroup("All Toggle")]
    [SerializeField] private ManagerButton _crabToggle;
    [SerializeField] private ManagerButton _fishToggle;
    [SerializeField] private ManagerButton _seahorseToggle;
    [SerializeField] private ManagerButton _octopusToggle;
    [SerializeField] private ManagerButton _turtleToggle;

    [Title("Black screen")]
    [SerializeField] private LeanToggle _blackScreen;

    private string _keyAllGames = "for.the.sea_minigames.allgames.4.99";

    private void Start()
    {
        var result = PlayerPrefs.GetInt(_keyAllGames);
        //EnableToggle(0);
        if (controller == null)
            InitializePurchasing();

        StartCoroutine(CheackInitializationPurshasingAllGames());
    }
    private void InitializePurchasing()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(_keyAllGames, ProductType.NonConsumable);
        UnityPurchasing.Initialize(this, builder);
    }
    private IEnumerator CheackInitializationPurshasingAllGames()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("AllGame " + ProductAllGame.hasReceipt);
        var product = ProductAllGame;

        if (product != null && product.hasReceipt)
        {
            EnableToggle(true);
        }
        else
        {
            EnableToggle(false);
        }

    }
    public void OnInitialized(IStoreController cont, IExtensionProvider ext)
    {
        controller = cont;
        extensions = ext;
    }

    public void PurshasingAllGames() => BuyProductID(_keyAllGames);
    
    private void EnableToggle(bool active)
    {
        _fishToggle.On(active);
        _seahorseToggle.On(active);
        _octopusToggle.On(active);
        _turtleToggle.On(active);

#if UNITY_EDITOR
        // PlayerPrefs.SetInt(_keyAllGames, active ? 1 : 0);
#else
        ES3.Save(_keyAllGames, active);
#endif
    }
    private void EnableToggle(int value)
    {
        var active = value == 1 ? true : false;
        _fishToggle.On(active);
        _seahorseToggle.On(active);
        _octopusToggle.On(active);
        _turtleToggle.On(active);

#if UNITY_EDITOR
        //PlayerPrefs.SetInt(_keyAllGames, active ? 1 : 0);
#else
        ES3.Save(_keyAllGames, active);
#endif
    }

    #region IListenerIAP
    [Button]
    public void OnInitializeFailed(InitializationFailureReason error) => InitializeFailed(error);
    public void OnInitializeFailed(InitializationFailureReason error, string message) => InitializeFailed(error, message);

    private void InitializeFailed(InitializationFailureReason error, string message = "")
    {
        _blackScreen.On = false;
    }
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        LoadingScene("1. Level select");
        return PurchaseProcessingResult.Complete;
    }
    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        LoadingScene("1. Level select");
    }

    private Product ProductAllGame => controller.products.WithID(_keyAllGames);
    #endregion


    public void LoadingScene(string nameScene)
    {
        Debug.Log(nameScene);
        if (nameScene.Contains("Crab") || ProductAllGame.hasReceipt)
        {
            StartCoroutine(LoadScene(nameScene));
        }
    }
    private IEnumerator LoadScene(string name)
    {
        _blackScreen.On = true;
        yield return new WaitForSeconds(1f);
        _loadScene.Load(name);
    }

    private bool IsInitialized => controller != null && extensions != null;

    public void BuyProductID(string productId)
    {
        _blackScreen.On = true;

        if (IsInitialized)
        {
            Product product = controller.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                controller.InitiatePurchase(product);
            }
            else
            {
                Notification("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                _blackScreen.On = false;
            }
        }
        else
        {
            Notification("BuyProductID FAIL. Not initialized.");
            _blackScreen.On = false;
        }
    }
    private void Notification(string error, string type = "Error") => ToastNotifications.Instance.Notify(new NotificationData(type, error));
}
