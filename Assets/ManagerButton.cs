using qASIC;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


public class ManagerButton : MonoBehaviour
{
    private enum TypePurchasing { Free, Purchasing }
    [SerializeField] private TypePurchasing purchasing;

    [Title("Name Load Scene")]
    [SerializeField] private string _loadScene;

    private Button _button;
    private Animator _animator;

    [SerializeField]
    [Title("Has the product been purchased?")]
    [LabelText("Purchased")]
    private bool isBuy = false;

    [Title("Blocked Image")]
    [SerializeField] private Image _purchasing;
    [SerializeField] private GameObject _purchasingPrefab;
    [SerializeField, ShowIf("purchasing", TypePurchasing.Purchasing)] private Image _block;
    [SerializeField, ShowIf("purchasing", TypePurchasing.Purchasing)] private GameObject _blockPrefab;

    [Title("Manager")]
    [SerializeField] private PurchasingManager _manager;
    
    private void Start()
    {
        if(_button == null) _button = GetComponent<Button>();
        if (_animator == null) _animator = GetComponentInChildren<Animator>();

        _button.onClick.AddListener(OnClickButton);
        On(isBuy);
    }
    public void OnClickButton()
    {
        if (purchasing == TypePurchasing.Free)
            _manager.LoadingScene(_loadScene);
        else
        {
            if (isBuy)
                _manager.LoadingScene(_loadScene);
            else
                _manager.PurshasingAllGames();
        }
    }

    public void On(bool value)
    {
        if (purchasing == TypePurchasing.Purchasing)
        {
            isBuy = value;

            if (!value)
            {
                ActiveIcon(false);
                _button.image = _block;
            }
            else
            {
                ActiveIcon(true);
                _animator.enabled = value;
                _button.image = _purchasing;
            }
        }
        else
        {
            _button.image = _purchasing;
            ActiveIcon(true);
        }
    }

    private void ActiveIcon(bool value)
    {
        if (purchasing == TypePurchasing.Purchasing)
        {
            _purchasingPrefab.SetActive(value);
            _blockPrefab.SetActive(!value);
        }
        else _purchasingPrefab.SetActive(true);
    }
}
