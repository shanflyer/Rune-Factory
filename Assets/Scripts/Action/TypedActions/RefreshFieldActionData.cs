// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RefreshField
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RefreshField")]
public class RefreshFieldActionData : GameActionBaseData
{
        public int fieldId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshField
        {
                fieldId = this.fieldId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.fieldId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
