using System.Collections;
using System.Drawing;
using UnityEngine;

public class GameGuidePanel : GamePanel<GuidStepData>
{
    [SerializeField]
    GuideButton guideButton;
    [SerializeField]
    Transform ring;
    protected override void Awake()
    {
        base.Awake();
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        guideButton = FindChildGameObject<GuideButton>("GuideButton");
        ring = FindChildGameObject("Ring");
    }
    public override void InitReferenceData(GuidStepData v)
    {
        base.InitReferenceData(v);
        ring.gameObject.SetActive(true);
        RectTransform rectTransform = guideButton.transform as RectTransform;
        rectTransform.sizeDelta = Vector2.zero ;
        ring.localScale = Vector2.zero;
        GameTimerController.instance.DelayAction(200, () =>
        {
            if (GameGuideManager.instance.GetSelectableSize(data.selectableId, out var pos, out var size))
            { 
                rectTransform.position = pos;
                rectTransform.sizeDelta = size;
                ring.localScale = new Vector3(size.x, size.x, 100);
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