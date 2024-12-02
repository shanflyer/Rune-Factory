using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GuideButton : Button
{
    /*
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        GameGuideManager.instance.GuideButtonAction(eventData);
    }*/
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData); 
        GameGuideManager.instance.GuideButtonAction(eventData);
    }
}