// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - DeleteMapItem
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/DeleteMapItem")]
public class DeleteMapItemActionData : GameActionBaseData
{
        public int mapItemInstanceId;
        public bool triggerClear;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DeleteMapItem
        {
                mapItemInstanceId = this.mapItemInstanceId,
                triggerClear = this.triggerClear
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
