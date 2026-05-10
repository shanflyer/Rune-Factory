// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TimeRun
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TimeRun")]
public class TimeRunActionData : GameActionBaseData
{
        public bool run;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TimeRun
        {
                run = this.run
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
