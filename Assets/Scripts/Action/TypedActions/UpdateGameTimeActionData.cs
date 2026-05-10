// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - UpdateGameTime
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/UpdateGameTime")]
public class UpdateGameTimeActionData : GameActionBaseData
{
        public int totalMinute;
        public int year;
        public int season;
        public int day;
        public int hour;
        public int minute;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new UpdateGameTime
        {
                totalMinute = this.totalMinute,
                year = this.year,
                season = this.season,
                day = this.day,
                hour = this.hour,
                minute = this.minute
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
