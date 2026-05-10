// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetFixedTime
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetFixedTime")]
public class SetFixedTimeActionData : GameActionBaseData
{
        public int date;
        public int hour;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetFixedTime
        {
                date = this.date,
                hour = this.hour
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
