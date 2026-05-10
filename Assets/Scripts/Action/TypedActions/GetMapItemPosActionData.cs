// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - GetMapItemPos
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/GetMapItemPos")]
public class GetMapItemPosActionData : GameActionBaseData
{
        public int mapItemIntanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new GetMapItemPos
        {
                mapItemIntanceId = this.mapItemIntanceId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
