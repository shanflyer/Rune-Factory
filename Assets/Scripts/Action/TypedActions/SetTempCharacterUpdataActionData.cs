// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetTempCharacterUpdata
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetTempCharacterUpdata")]
public class SetTempCharacterUpdataActionData : GameActionBaseData
{
        public bool canUpdata;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetTempCharacterUpdata
        {
                canUpdata = this.canUpdata
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
