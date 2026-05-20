// container - ChangeMapRoom
using System;
using UnityEngine;

public class ChangeMapRoomNode : ContainerNode
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
            newRoom = this.newRoom,
            setValue = setValue,
            setResult = setResult
        };
            if (source != int.MinValue && source != 0) action.oldRoom = source;
            if (target != int.MinValue && target != 0) action.newRoom = source;
        ExecuteChildren(source, target, value, setResult, setValue, immediately);
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
