// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SkillPauseAction
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SkillPauseAction")]
public class SkillPauseActionActionData : GameActionBaseData
{
        public bool pause;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SkillPauseAction
        {
                pause = this.pause
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
