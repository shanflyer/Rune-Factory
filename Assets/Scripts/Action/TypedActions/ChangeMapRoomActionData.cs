// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ChangeMapRoom
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ChangeMapRoom")]
public class ChangeMapRoomActionData : GameActionBaseData
{
        public int oldRoom;
        public int newRoom;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeMapRoom
        {
                oldRoom = this.oldRoom,
                newRoom = this.newRoom
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.newRoom = source;
            if (source != 0 && source != int.MinValue) action.oldRoom = source;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
