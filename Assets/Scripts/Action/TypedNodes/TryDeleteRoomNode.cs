// TryDeleteRoom
using System;
using UnityEngine;

public class TryDeleteRoomNode : ActionNode
{
        public int roomId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryDeleteRoom
        {
                roomId = this.roomId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.roomId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}