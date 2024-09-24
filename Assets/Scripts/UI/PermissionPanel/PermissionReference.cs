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

    public override async Task InitData(Permission t, SelectAction<Permission> SelectAction = null, ToggleGroup toggleGroup = null)
    {
       await  base.InitData(t, SelectAction, toggleGroup);
        Name.text = data.permissionData.permissionName;
        ConditionValue.text = data.permissionData.conditionStr;
        RewardValue.text = data.permissionData.reward.ToString();
        GetTips.localScale = data.isGet ? Vector3.one : Vector3.zero;
    }
}