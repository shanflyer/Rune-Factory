using System.Collections;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;

public class GameGuidePanel : GamePanel<GuidStepData>
{
    [SerializeField]
    GuideButton guideButton;
    [SerializeField]
    Transform ring;
    [SerializeField]
    Image icon;
    protected override void Awake()
    {
        base.Awake();
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        guideButton = FindChildGameObject<GuideButton>("GuideButton");
        ring = FindChildGameObject("Ring");
        icon = FindChildGameObject<Image>("Icon");
    }
    public override void InitReferenceData(GuidStepData v)
    {
        base.InitReferenceData(v);
        ring.gameObject.SetActive(true);
        RectTransform rectTransform = guideButton.transform as RectTransform;
        rectTransform.sizeDelta = Vector2.zero ;
        ring.localScale = Vector2.zero;
        icon.enabled = false;
        GameTimerController.instance.DelayAction(200, () =>
        {
            if (GameGuideManager.instance.GetSelectableSize(data.selectableId, out var pos, out var size))
            { 
                rectTransform.position = pos;
                rectTransform.sizeDelta = size;
                float ringSize = (size.x > size.y ? size.x : size.y)*0.5f;
                ring.localScale = new Vector3(ringSize, ringSize, 100);
                icon.enabled = true;
               // Debug.Log($"guideButton:{guideButton.transform.position}");
            }
        });
    }
    public override void Close()
    {
        base.Close();
        ring.gameObject.SetActive(false);
    }

}