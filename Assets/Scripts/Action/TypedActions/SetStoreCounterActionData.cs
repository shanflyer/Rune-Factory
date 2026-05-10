// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetStoreCounter
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetStoreCounter")]
public class SetStoreCounterActionData : GameActionBaseData
{
        public int nullAction;
        public int storeCounterId;
        public int playerId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetStoreCounter
        {
                nullAction = this.nullAction,
                storeCounterId = this.storeCounterId,
                playerId = this.playerId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.playerId = source;
            if (target != 0 && target != int.MinValue) action.storeCounterId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
