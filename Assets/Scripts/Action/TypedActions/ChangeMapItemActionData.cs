// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ChangeMapItem
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ChangeMapItem")]
public class ChangeMapItemActionData : GameActionBaseData
{
        public int itemId;
        public int newDataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeMapItem
        {
                itemId = this.itemId,
                newDataId = this.newDataId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
