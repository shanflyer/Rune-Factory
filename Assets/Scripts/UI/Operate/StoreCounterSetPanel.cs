using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreCounterSetPanel : GamePanel<IReferenceData>
{
    Image sellItemIcon;
    TextMeshProUGUI sellItemName;
    TextMeshProUGUI priceValue;
    TextMeshProUGUI sellCount;
    Button reduceButton, addButton,topButton;
    Button changeItemButton, getItemDownButton;
    Button returnButton;
    protected override void Awake()
    {
        base.Awake();
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
    }
    public override void InitReferenceData(IReferenceData v)
    {
        base.InitReferenceData(v);
    }
}