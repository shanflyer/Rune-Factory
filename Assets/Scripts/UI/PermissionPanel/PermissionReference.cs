using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PermissionReference : UIObjReference<Permission>
{
    [SerializeField]
    private TextMeshProUGUI Name;

    [SerializeField]
    private TextMeshProUGUI ConditionValue;

    [SerializeField]
    private TextMeshProUGUI RewardValue;

    [SerializeField]
    private Transform GetTips;

    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        Name = FindChildGameObject<TextMeshProUGUI>("Name");
        ConditionValue = FindChildGameObject<TextMeshProUGUI>("ConditionValue");
        RewardValue = FindChildGameObject<TextMeshProUGUI>("RewardValue");
        GetTips = FindChildGameObject("GetTips");
    }

    public override void InitData(Permission t, SelectAction<Permission> SelectAction = null, ToggleGroup toggleGroup = null)
    {
       base.InitData(t, SelectAction, toggleGroup);
        Name.SetSWText(data.permissionData.permissionName);
        ConditionValue.SetSWText(data.permissionData.conditionStr);
        RewardValue.SetSWText(data.permissionData.reward);
        GetTips.localScale = data.isGet ? Vector3.one : Vector3.zero;
    }
}