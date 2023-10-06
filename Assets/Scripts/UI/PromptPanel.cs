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
    public override Task InitData(string dataKey)
    {
        info.text = dataKey;
        return base.InitData(dataKey); 
    }
}
