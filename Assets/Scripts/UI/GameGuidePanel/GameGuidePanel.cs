using System.Collections;
using UnityEngine;

public class GameGuidePanel : GamePanel<GuidStepData>
{
    [SerializeField]
    GuideButton guideButton;
    protected override void Awake()
    {
        base.Awake();
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        guideButton = FindChildGameObject<GuideButton>("GuideButton");
    }
    public override void InitReferenceData(GuidStepData v)
    {
        base.InitReferenceData(v);
        if(GameGuideManager.instance.GetSelectableSize(data.selectableId,out var pos,out var size))
        {
            guideButton.transform.position = pos;
            (guideButton.transform as RectTransform).sizeDelta = size;
        }
    }

}