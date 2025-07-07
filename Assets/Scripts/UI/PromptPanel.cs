using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class PromptPanel : GamePanel<IReferenceData>
{
    TextMeshProUGUI info;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
    }
    public override void InitData(string dataKey)
    {
        info.SetSWText(dataKey);
        base.InitData(dataKey); 
    }
}
