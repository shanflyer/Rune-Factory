using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPPrivacyLink : MonoBehaviour, IPointerClickHandler
{
    public string privacyUrl = "https://shanflyer.github.io/FantasyTown-EveryDay-Privacy/";
    TextMeshProUGUI _tmp;

    void Awake() => _tmp = GetComponent<TextMeshProUGUI>();

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_tmp, eventData.position, eventData.pressEventCamera);
        if (linkIndex != -1)
        {
            var info = _tmp.textInfo.linkInfo[linkIndex];
            if (info.GetLinkID() == "privacy")
                Application.OpenURL(privacyUrl);
        }
    }
}
