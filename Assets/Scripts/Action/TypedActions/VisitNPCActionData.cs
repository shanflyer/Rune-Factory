// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - VisitNPC
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/VisitNPC")]
public class VisitNPCActionData : GameActionBaseData
{
        public int sourceId;
        public int targetId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new VisitNPC
        {
                sourceId = this.sourceId,
                targetId = this.targetId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.sourceId = source;
            if (target != 0 && target != int.MinValue) action.targetId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
