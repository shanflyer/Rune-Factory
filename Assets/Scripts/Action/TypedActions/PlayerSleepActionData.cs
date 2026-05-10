// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - PlayerSleep
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/PlayerSleep")]
public class PlayerSleepActionData : GameActionBaseData
{
        public int characterId;
        public int targetHour;
        public int targetMinute;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new PlayerSleep
        {
                characterId = this.characterId,
                targetHour = this.targetHour,
                targetMinute = this.targetMinute
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.targetHour = target;
            if (value != -1) action.targetMinute = value;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
