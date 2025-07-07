using UnityEngine;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.UI; 

public class FunctionInfoReference:UIObjReference<InfoData>
{
    [SerializeField]
    private TextMeshProUGUI text;
    public override void InitChildObjData()
    {
        base.InitChildObjData();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }
 
    public override void InitData(string dataKey)
    {
        text.SetSWText(dataKey);
        base.InitData(dataKey);
    }
    public override void InitData(InfoData t, SelectAction<InfoData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        text.SetSWText(t.text);
        base.InitData(t, SelectAction, toggleGroup);
    }
}