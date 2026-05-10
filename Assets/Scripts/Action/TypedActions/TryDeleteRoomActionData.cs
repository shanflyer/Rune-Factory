// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TryDeleteRoom
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TryDeleteRoom")]
public class TryDeleteRoomActionData : GameActionBaseData
{
        public int roomId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryDeleteRoom
        {
                roomId = this.roomId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.roomId = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
