// TryCreatRoom
using System;
using UnityEngine;

public class TryCreatRoomNode : ActionNode
{
        public int roomId;
        public string roomName;
        public int eventId;
        public int instance;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryCreatRoom
        {
                roomId = this.roomId,
                roomName = this.roomName,
                eventId = this.eventId,
                instance = this.instance,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.roomId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
