// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetFixedPlayerShaderPos
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetFixedPlayerShaderPos")]
public class SetFixedPlayerShaderPosActionData : GameActionBaseData
{
        public bool fixedPos;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetFixedPlayerShaderPos
        {
                fixedPos = this.fixedPos
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
