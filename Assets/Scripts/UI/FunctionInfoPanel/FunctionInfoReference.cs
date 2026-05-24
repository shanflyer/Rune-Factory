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

    public override Task InitData(string dataKey)
    {
        text.SetSWText(dataKey);
        return base.InitData(dataKey);
    }
    public override Task InitData(InfoData t, SelectAction<InfoData> SelectAction = null, ToggleGroup toggleGroup = null)
    {
        text.SetSWText(t.text);
        return base.InitData(t, SelectAction, toggleGroup);
    }
}
