using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class MapObjTipsPanel : GamePanel<IReferenceData>
{
    [SerializeField]
    TextMeshProUGUI infoText;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        infoText = FindChildGameObject<TextMeshProUGUI>("Info");
    }
    public override Task InitData(string dataKey)
    {
        infoText.text = dataKey;
        return base.InitData(dataKey);
    }  

}
