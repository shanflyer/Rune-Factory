// TryCreatPasture
using System;
using UnityEngine;

public class TryCreatPastureNode : ActionNode
{
        public int roomId;
        public int itemInstanceId;
        public string pastureName;
        public int dataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryCreatPasture
        {
                roomId = this.roomId,
                itemInstanceId = this.itemInstanceId,
                pastureName = this.pastureName,
                dataId = this.dataId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.roomId = source;
            if (target != 0 && target != int.MinValue) action.itemInstanceId = target;
            if (value != -1) action.dataId = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}