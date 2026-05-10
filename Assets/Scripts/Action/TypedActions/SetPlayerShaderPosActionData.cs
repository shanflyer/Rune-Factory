// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetPlayerShaderPos
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetPlayerShaderPos")]
public class SetPlayerShaderPosActionData : GameActionBaseData
{
        public Vector3 pos;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetPlayerShaderPos
        {
                pos = this.pos
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
