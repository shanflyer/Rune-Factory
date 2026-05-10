// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ClearOverrideEnvironment
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ClearOverrideEnvironment")]
public class ClearOverrideEnvironmentActionData : GameActionBaseData
{


    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ClearOverrideEnvironment
        {

        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
