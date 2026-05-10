// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RemoveGameEvent
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RemoveGameEvent")]
public class RemoveGameEventActionData : GameActionBaseData
{
        public int eventId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemoveGameEvent
        {
                eventId = this.eventId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.eventId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
