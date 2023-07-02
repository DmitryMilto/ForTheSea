using System;
using UnityEngine;

public interface INotifiable
{
    public void Notify(string message);
    public void Notify(NotificationData messageContext);

    [Serializable] public class NotificationData
    {
        public string Type
        {
            get => type;
            protected internal set => type = value;
        } [SerializeField] private string type;
        public string Header
        {
            get => header;
            protected internal set => header = value;
        } [SerializeField] private string header;
        public string Message
        {
            get => message;
            protected internal set => message = value;
        } [SerializeField] private string message;
        public Sprite Icon
        {
            get => icon;
            protected internal set => icon = value;
        } [SerializeField] private Sprite icon;

        public NotificationData(string type, string message)
        {
            Type = type;
            Message = message;
        }
    }
}
