// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - StopSetTempMapItem
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/StopSetTempMapItem")]
public class StopSetTempMapItemActionData : GameActionBaseData
{
        public int instanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new StopSetTempMapItem
        {
                instanceId = this.instanceId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (target != 0 && target != int.MinValue) action.instanceId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
