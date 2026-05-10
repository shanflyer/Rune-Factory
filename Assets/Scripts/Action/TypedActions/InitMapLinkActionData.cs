// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - InitMapLink
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/InitMapLink")]
public class InitMapLinkActionData : GameActionBaseData
{
        public int linkInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new InitMapLink
        {
                linkInstanceId = this.linkInstanceId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.linkInstanceId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
