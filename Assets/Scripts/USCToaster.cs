using UnityEngine;
using static UnityServicesController;

public class USCToaster : MonoBehaviour
{
    public void OnInitialize(USCEvents arg) => ToastNotifications.Instance.Notify(arg.Message);

    public void OnError(USCEvents arg) => ToastNotifications.Instance.Notify(new INotifiable.NotificationData("Error", arg.Message));
}
