using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarehousePanel : GamePanel<IReferenceData>
{
    [SerializeField]
    PlayerEquipQuickReference playerEquipQuickReference;
    [SerializeField]

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();

    }
    public override void InitReferenceData(IReferenceData v)
    {
        base.InitReferenceData(v);
    }

}
