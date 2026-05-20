// LerpGameTime
using System;
using UnityEngine;

public class LerpGameTimeNode : ActionNode
{
        public int targetHour;
        public int targetMinute;
        public float totalTime;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new LerpGameTime
        {
                targetHour = this.targetHour,
                targetMinute = this.targetMinute,
                totalTime = this.totalTime,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
