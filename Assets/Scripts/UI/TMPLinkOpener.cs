using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class TMPLinkOpener : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text _text;
    private Canvas _canvas;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>(); // TextMeshProUGUI
        _canvas = GetComponentInParent<Canvas>(); // 找到所在 Canvas
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 确保最新排版网格（动态改文本必须调用）
        _text.ForceMeshUpdate();

        // Overlay 画布传 null，Camera/World 画布传有效相机
        Camera cam = null;
        if (_canvas && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;

        var linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, eventData.position, cam);
        if (linkIndex == -1) return; // 没点到链接

        var linkInfo = _text.textInfo.linkInfo[linkIndex];
        var id = linkInfo.GetLinkID(); // 取 <link="..."> 的内容

        // 如需深链优先，这里判断自定义 id 再决定打开 App 或网页
        Application.OpenURL(id);
    }
}