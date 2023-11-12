using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PermissionReference : UIObjReference<Permission>
{
    [SerializeField]
    TextMeshProUGUI Name;
    [SerializeField]
    TextMeshProUGUI ConditionValue;
    [SerializeField]
    TextMeshProUGUI RewardValue;
    [SerializeField]
    Transform GetTips;
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
        Name.text = data.permissionData.permissionName;
        ConditionValue.text = data.permissionData.conditionStr;
        RewardValue.text = data.permissionData.reward.ToString();
        GetTips.localScale = data.isGet ? Vector3.one : Vector3.zero;
    }
}
