// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ClearManufature
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ClearManufature")]
public class ClearManufatureActionData : GameActionBaseData
{
        public int manufatureId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ClearManufature
        {
                manufatureId = this.manufatureId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
