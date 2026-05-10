// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - CreatTeamPlayer
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/CreatTeamPlayer")]
public class CreatTeamPlayerActionData : GameActionBaseData
{
        public bool holdDisplay;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatTeamPlayer
        {
                holdDisplay = this.holdDisplay
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
