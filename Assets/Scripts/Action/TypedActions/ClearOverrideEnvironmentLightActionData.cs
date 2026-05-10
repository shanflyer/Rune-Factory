// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ClearOverrideEnvironmentLight
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ClearOverrideEnvironmentLight")]
public class ClearOverrideEnvironmentLightActionData : GameActionBaseData
{


    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ClearOverrideEnvironmentLight
        {

        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
