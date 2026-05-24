// RefreshPasture
using System;
using UnityEngine;

public class RefreshPastureNode : ActionNode
{
        public int instanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshPasture
        {
                instanceId = this.instanceId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.instanceId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
