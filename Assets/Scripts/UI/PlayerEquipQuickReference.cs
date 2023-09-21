 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEquipQuickReference : UIObjReference<IReferenceData>
{
    [SerializeField]
    Dropdown playerSelcet; 
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
    }
}
