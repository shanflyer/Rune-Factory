using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class MapObjTipsPanel : GamePanel<IReferenceData>
{
    public override bool changeInputModel => false;
    [SerializeField]
    TextMeshProUGUI infoText;
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        infoText = FindChildGameObject<TextMeshProUGUI>("Info");
    }
    public override void InitData(string dataKey)
    {
        infoText.SetSWText(dataKey);
        base.InitData(dataKey);
    }  

}
