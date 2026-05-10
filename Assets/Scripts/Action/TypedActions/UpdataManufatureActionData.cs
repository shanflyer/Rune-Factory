// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - UpdataManufature
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/UpdataManufature")]
public class UpdataManufatureActionData : GameActionBaseData
{
        public int manufatureId;
        public int waitTime;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new UpdataManufature
        {
                manufatureId = this.manufatureId,
                waitTime = this.waitTime
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
