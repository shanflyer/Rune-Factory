using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GuideButton : GameButton
{
    /*
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        GameGuideManager.instance.GuideButtonAction(eventData);
    }*/
    protected override void OnEnable()
    {
        base.OnEnable();
        onClick.AddListener(ClickAction);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        onClick.RemoveListener(ClickAction);
    }
    void ClickAction()
    {
        GameGuideManager.instance.GuideButtonAction();
    }
    /*
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData); 
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            GameGuideManager.instance.GuideButtonAction(eventData);
        } 
    }*/
}
