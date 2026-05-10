// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - HidePanelGroup
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/HidePanelGroup")]
public class HidePanelGroupActionData : GameActionBaseData
{
        public bool hide;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new HidePanelGroup
        {
                hide = this.hide
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
