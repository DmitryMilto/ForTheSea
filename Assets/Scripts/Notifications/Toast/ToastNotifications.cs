using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using LeanPool = Lean.Pool.LeanPool;
using NotificationData = INotifiable.NotificationData;

[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
public class ToastNotifications : Singleton<ToastNotifications>, INotifiable
{
    [SerializeField] private NotificationLibrary_Toast notificationLibrary;
    public RectTransform Container => container; [FoldoutGroup("Container settings"), SerializeField] private RectTransform container;

    [FoldoutGroup("Container settings/Transform"), SerializeField, LabelText("Anchors (Xmin,Ymin,Xmax,Ymax)")] private Vector4 container_anchorsMinMax = new (0, 0, 1, 1);
    [FoldoutGroup("Container settings/Transform"), SerializeField, LabelText("Size delta")] private Vector2 container_sizeDelta = Vector2.zero;

    [FoldoutGroup("Container settings/Layout"), SerializeField, LabelText("Alignment")] private TextAnchor container_alignment = TextAnchor.LowerRight;
    [FoldoutGroup("Container settings/Layout"), SerializeField, LabelText("Spacing")] private int container_spacing = 10;
    [FoldoutGroup("Container settings/Layout"), SerializeField, LabelText("Padding (Left,Right,Top,Down)")] private Vector4 container_padding = new (50, 50, 50, 50);


    protected override void Awake()
    {
        base.Awake();
        InitCanvas();
        CheckAndInitContainer(container_anchorsMinMax, container_sizeDelta);
        InitLayout(container_alignment, container_spacing, container_padding);

        void InitCanvas()
        {
            var canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 0;
            //canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.Normal &
            //                                  AdditionalCanvasShaderChannels.Tangent &
            //                                  AdditionalCanvasShaderChannels.TexCoord1 &
            //                                  AdditionalCanvasShaderChannels.TexCoord2 &
            //                                  AdditionalCanvasShaderChannels.TexCoord3;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1;
        }
        void CheckAndInitContainer(Vector4 anchors, Vector4 sizeDelta)
        {
            if (!container) container = new GameObject("Container", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            container.SetParent(transform);
            container.anchorMin = new Vector2(anchors.x,anchors.y);
            container.anchorMax = new Vector2(anchors.z,anchors.w);
            container.sizeDelta = sizeDelta;
        }
        void InitLayout(TextAnchor alignment, int spacing, Vector4 padding)
        {
            if (container.TryGetComponent(out VerticalLayoutGroup layout) is false) layout = container.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = alignment;
            layout.spacing = spacing;
            layout.padding = new RectOffset(Mathf.RoundToInt(padding.x), Mathf.RoundToInt(padding.y), Mathf.RoundToInt(padding.z), Mathf.RoundToInt(padding.w));
            layout.reverseArrangement = false;
            layout.childControlWidth = layout.childControlHeight = false;
            layout.childScaleWidth = layout.childScaleHeight = false;
            layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        }
    }

    public void Notify(string message) => Notify(new NotificationData(type: notificationLibrary.Library.First().Type, message: message));

    [Button] public void Notify(NotificationData messageContext)
    {
        var toastTemplate = notificationLibrary.Library.FirstOrDefault(pair => pair.Type == messageContext.Type) ?? notificationLibrary.Library.First();
        var instance = LeanPool.Spawn(toastTemplate.Prefab, container);
        var instance_linker = instance.GetComponent<NotificationLinker_Toast>();

        instance_linker.Header.text = string.IsNullOrEmpty(messageContext.Header) is false ? messageContext.Header : toastTemplate.Linker.Header.text;
        instance_linker.Message.text = string.IsNullOrEmpty(messageContext.Message) is false ? messageContext.Message : toastTemplate.Linker.Message.text;
        instance_linker.Icon.sprite = messageContext.Icon ?? toastTemplate.Linker.Icon.sprite;

        switch (toastTemplate.Type)
        {
            case "Warning" or "Warn": Debug.LogWarning($"[Toast] \"{GetHeaderOrDefault()}\" - \"{messageContext.Message}\""); break;
            case "Error" or "Err": Debug.LogError($"[Toast] \"{GetHeaderOrDefault()}\" - \"{messageContext.Message}\""); break;
            default: Debug.Log($"[Toast] \"{GetHeaderOrDefault()}\" - \"{messageContext.Message}\""); break;
        }

        string GetHeaderOrDefault() => (string.IsNullOrEmpty(messageContext.Header) ? notificationLibrary.Library.Find(item => item.Type == messageContext.Type).Linker.Header.text : messageContext.Header);
    }
}