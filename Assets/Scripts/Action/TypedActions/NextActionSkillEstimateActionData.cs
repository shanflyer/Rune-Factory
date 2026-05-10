// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - NextActionSkillEstimate
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/NextActionSkillEstimate")]
public class NextActionSkillEstimateActionData : GameActionBaseData
{
        public int skillId;
        public int sourceId;
        public bool displayHurt;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new NextActionSkillEstimate
        {
                skillId = this.skillId,
                sourceId = this.sourceId,
                displayHurt = this.displayHurt
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
