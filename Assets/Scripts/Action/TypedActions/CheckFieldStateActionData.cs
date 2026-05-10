// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - CheckFieldState
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/CheckFieldState")]
public class CheckFieldStateActionData : GameActionBaseData
{
        public int instanceid;
        public bool plantDeath;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CheckFieldState
        {
                instanceid = this.instanceid,
                plantDeath = this.plantDeath
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.instanceid = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
