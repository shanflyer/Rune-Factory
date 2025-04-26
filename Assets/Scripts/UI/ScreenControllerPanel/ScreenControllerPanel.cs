using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScreenControllerPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    private RectTransform JoyStick;
    [SerializeField]
    private MyJoyStick MyJoyStick;
    [SerializeField]
    private TouchArea touchArea;
    [SerializeField]
    private Vector2 defaultPosition;
    [SerializeField]
    private RectTransform panelRect;
   
    public void RefreshJoyStickColor()
    {
        MyJoyStick.RefreshJoyStickColor();
    }
    public override void InitReferenceData(IReferenceData v)
    {
        RefreshJoyStickColor();
        base.InitReferenceData(v); 
    }
    public override Task InitData(string dataKey)
    {
        RefreshJoyStickColor();
        return base.InitData(dataKey);
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        JoyStick = FindChildGameObject("JoyStick") as RectTransform;
        touchArea = FindChildGameObject<TouchArea>("TouchArea");
        defaultPosition = JoyStick.anchoredPosition;
        panelRect = transform as RectTransform;
        MyJoyStick = FindChildGameObject<MyJoyStick>("Stick"); 
    }
    protected override void Awake()
    {
        base.Awake();
        touchArea.PointerDownDele = SetPointerDown;
        touchArea.PointerUpDele = SetPointerUp;
        touchArea.PointerDragDele = OnDrag;
        
    }
    private void SetPointerDown(PointerEventData eventData)
    {
        JoyStick.position = CameraManager.ScreenPointToUIPoint(JoyStick, eventData.position);
      
        MyJoyStick.OnPointerDown(eventData); 
    }
    void OnDrag(PointerEventData eventData)
    {
        MyJoyStick.OnDrag(eventData);
    }
    private void SetPointerUp(PointerEventData eventData)
    {
        JoyStick.anchoredPosition = defaultPosition;
        MyJoyStick.OnPointerUp(eventData);
       
    }
}
