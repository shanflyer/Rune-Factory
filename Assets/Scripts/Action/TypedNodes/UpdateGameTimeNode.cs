// UpdateGameTime
using System;
using UnityEngine;

[Serializable]
public class UpdateGameTimeNode : ActionNode
{
        public int totalMinute;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new UpdateGameTime
        {
                totalMinute = this.totalMinute,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
