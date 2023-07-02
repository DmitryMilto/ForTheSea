using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class TextMeshPro_Text_LinkListener : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text text => _text ??= GetComponent<TMP_Text>(); [SerializeField] private TMP_Text _text;

    public void OpenURL(string linkID, string linkText, int linkIndex)
    {
        //Debug.Log($"linkID = \"{linkID}\", linkText = \"{linkText}\", linkIndex = \"{linkIndex}\"");
        Application.OpenURL(linkText.StartsWith("http://") ? linkText : $"http://{linkText}");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, text.canvas.worldCamera);

        if (linkIndex < 0)
        {
            Debug.Log($"Click detected, but raycast returned that it's not the link");
            return;
        }

        var linkInfo = text.textInfo.linkInfo[linkIndex];
        var linkId = linkInfo.GetLinkID();
        var linkText = linkInfo.GetLinkText();

        OpenURL(linkId, linkText, linkIndex);
    }
}
