using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void OnPointerDele(PointerEventData eventData);
public class TouchArea :MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public OnPointerDele PointerDownDele, PointerUpDele,PointerDragDele;

    public void OnDrag(PointerEventData eventData)
    {
        if (PointerDragDele != null)
        {
            PointerDragDele(eventData);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (PointerDownDele != null)
        {
            PointerDownDele(eventData);
        } 
        //Debug.Log($"Down position:{eventData.position}--pressPosition:{eventData.pressPosition}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (PointerUpDele != null)
        {
            PointerUpDele(eventData);
        }
        //Debug.Log($"Up position:{eventData.position}--pressPosition:{eventData.pressPosition}");
    } 
}
