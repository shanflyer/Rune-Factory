using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void Vector2Delegate(Vector2 value);
public class RectRangeView : MonoBehaviour, IDragHandler 
{
    [SerializeField]
    private Transform point;
    [SerializeField]
    private Vector2 value;
    public Vector2 m_Value
    {
        get
        {
          return  value;
        }
        set
        {
            if (this.value != value)
            {
                RectTransform rectTransform = transform as RectTransform;
                this.value = value;
                float posX = (value.x-0.5f) * rectTransform.sizeDelta.x ;
                float posY = (value.y - 0.5f)  * rectTransform.sizeDelta.y ;
                point.localPosition = new Vector3(posX, posY, 0);

                if (vector2Delegate != null)
                {
                    vector2Delegate.Invoke(value);
                }
            }
        }
    }
    public Vector2Delegate vector2Delegate;

 

    public void OnDrag(PointerEventData eventData)
    {
        
        RectTransform rectTransform = transform as RectTransform; 
        var pos = eventData.position;
        CameraManager.ScreenPointToUILocalPoint(rectTransform, pos, out var localPos);
        //point.localPosition = localPos;
        
        float x = localPos.x / rectTransform.sizeDelta.x + 0.5f;
        float y = localPos.y / rectTransform.sizeDelta.y + 0.5f;
        x=math.clamp(x, 0f, 1f);
        y = math.clamp(y, 0f, 1f);
        m_Value = new Vector2(x, y);
    }
    
}