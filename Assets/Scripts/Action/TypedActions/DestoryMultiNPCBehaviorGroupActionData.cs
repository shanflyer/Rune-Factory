// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - DestoryMultiNPCBehaviorGroup
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/DestoryMultiNPCBehaviorGroup")]
public class DestoryMultiNPCBehaviorGroupActionData : GameActionBaseData
{
        public int groupId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DestoryMultiNPCBehaviorGroup
        {
                groupId = this.groupId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
