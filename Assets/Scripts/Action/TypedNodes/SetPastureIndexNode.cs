// SetPastureIndex
using System;
using UnityEngine;

public class SetPastureIndexNode : ActionNode
{
        public int pastureId;
        public int index;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetPastureIndex
        {
                pastureId = this.pastureId,
                index = this.index,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.pastureId = source;
            if (target != 0 && target != int.MinValue) action.index = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}