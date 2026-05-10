// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - AttachMapItemData
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/AttachMapItemData")]
public class AttachMapItemDataActionData : GameActionBaseData
{
        public int mapItemIntanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new AttachMapItemData
        {
                mapItemIntanceId = this.mapItemIntanceId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
