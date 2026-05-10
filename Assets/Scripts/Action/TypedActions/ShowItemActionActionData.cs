// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ShowItemAction
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ShowItemAction")]
public class ShowItemActionActionData : GameActionBaseData
{
        public int itemId;
        public int mapInstanceId;
        public Vector3 position;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ShowItemAction
        {
                itemId = this.itemId,
                mapInstanceId = this.mapInstanceId,
                position = this.position
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
