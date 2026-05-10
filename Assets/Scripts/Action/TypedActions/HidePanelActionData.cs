// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - HidePanel
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/HidePanel")]
public class HidePanelActionData : GameActionBaseData
{
        public bool hide;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new HidePanel
        {
                hide = this.hide
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
