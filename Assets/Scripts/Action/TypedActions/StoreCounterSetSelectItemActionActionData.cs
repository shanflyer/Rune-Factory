// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - StoreCounterSetSelectItemAction
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/StoreCounterSetSelectItemAction")]
public class StoreCounterSetSelectItemActionActionData : GameActionBaseData
{
        public int targetObj;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new StoreCounterSetSelectItemAction
        {
                targetObj = this.targetObj
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (target != 0 && target != int.MinValue) action.targetObj = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
