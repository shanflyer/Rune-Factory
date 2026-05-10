// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - DisplayHurt
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/DisplayHurt")]
public class DisplayHurtActionData : GameActionBaseData
{
        public int targetId;
        public string hurtValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DisplayHurt
        {
                targetId = this.targetId,
                hurtValue = this.hurtValue
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
