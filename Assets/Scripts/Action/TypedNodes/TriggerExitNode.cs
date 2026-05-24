// TriggerExit
using System;
using UnityEngine;

public class TriggerExitNode : ActionNode
{
        public int eventId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TriggerExit
        {
                eventId = this.eventId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
