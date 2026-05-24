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
    public override Task InitData(string dataKey)
    {
        infoText.SetSWText(dataKey);
        return base.InitData(dataKey);
    }

}
