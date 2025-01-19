
using UnityEngine;
using UnityEngine.UI;
public  class CommitterPanel:GamePanel<IReferenceData>
{
    [SerializeField]
    Button CloseBtn;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        CloseBtn = FindChildGameObject<Button>("Close");
    }
    protected override void Awake()
    {
        base.Awake(); 
        CloseBtn.onClick.AddListener(Close);
    }
}