// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - CheckGameGuideAction
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/CheckGameGuideAction")]
public class CheckGameGuideActionActionData : GameActionBaseData
{
        public int guidKey;
        public bool isEnd;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CheckGameGuideAction
        {
                guidKey = this.guidKey,
                isEnd = this.isEnd
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.guidKey = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
