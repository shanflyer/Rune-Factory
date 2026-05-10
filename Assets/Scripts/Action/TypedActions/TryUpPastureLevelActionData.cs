// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TryUpPastureLevel
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TryUpPastureLevel")]
public class TryUpPastureLevelActionData : GameActionBaseData
{
        public int pastureId;
        public int itemInstance;
        public int roomId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryUpPastureLevel
        {
                pastureId = this.pastureId,
                itemInstance = this.itemInstance,
                roomId = this.roomId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.pastureId = source;
            if (target != 0 && target != int.MinValue) action.itemInstance = target;
            if (value != -1) action.roomId = value;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
