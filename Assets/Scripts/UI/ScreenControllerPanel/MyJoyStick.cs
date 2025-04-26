using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
 
public class MyJoyStick : UIObjReference<IReferenceData>, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField]
    private float movementRange;

    private Camera uiCamera;

    [SerializeField]
    private Image JoyBg, JoyStickImage; 

    Vector2Delegate moveDelegate;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        JoyStickImage = GetComponent<Image>();
        JoyBg = transform.parent.GetComponent<Image>();
         
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveStick(eventData.position);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Color color = UIManager.instance.JoyStickColor;
        color.a= color.a*2;
        JoyBg.color = JoyStickImage.color = color;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ((RectTransform)transform).anchoredPosition = m_StartPos;

        Color color = UIManager.instance.JoyStickColor; 
        JoyBg.color = JoyStickImage.color = color;
        moveDelegate(Vector2.zero);
    }

    private Vector2 m_PointerDownPos, m_StartPos;
    public void RefreshJoyStickColor()
    {
        Color color = UIManager.instance.JoyStickColor;
        JoyBg.color = JoyStickImage.color = color;
    }
    private void MoveStick(Vector2 pointerPosition)
    {
        var canvasRect = transform.parent?.GetComponentInParent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("OnScreenStick needs to be attached as a child to a UI Canvas to function properly.");
            return;
        }
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, uiCamera, out var position);
        var delta = position - m_PointerDownPos;

        delta = Vector2.ClampMagnitude(delta, movementRange);
        ((RectTransform)transform).anchoredPosition = (Vector2)m_StartPos + delta;
        var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);
        if (moveDelegate != null)
        {
            moveDelegate(newPos.normalized);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        m_StartPos = ((RectTransform)transform).anchoredPosition;
        m_PointerDownPos = m_StartPos;
        uiCamera = CameraManager.instance.uiCamera;
        moveDelegate = CharacterManager.instance.SetControllerCharacterMoveDirection;
    }
}