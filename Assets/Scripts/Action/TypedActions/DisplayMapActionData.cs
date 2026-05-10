// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - DisplayMap
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/DisplayMap")]
public class DisplayMapActionData : GameActionBaseData
{
        public int displayMap;
        public int actionId;
        public bool fixedDisplay;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DisplayMap
        {
                displayMap = this.displayMap,
                actionId = this.actionId,
                fixedDisplay = this.fixedDisplay
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.displayMap = source;
            if (target != 0 && target != int.MinValue) action.actionId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
