// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - OpenOrCloseInputMap
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/OpenOrCloseInputMap")]
public class OpenOrCloseInputMapActionData : GameActionBaseData
{
        public bool open;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new OpenOrCloseInputMap
        {
                open = this.open
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
