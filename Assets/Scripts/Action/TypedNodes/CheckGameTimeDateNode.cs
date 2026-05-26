// CheckGameTimeDate
using System;
using UnityEngine;

[Serializable]
public class CheckGameTimeDateNode : ActionNode
{
        public int year;
        public int momth;
        public int day;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CheckGameTimeDate
        {
                year = this.year,
                momth = this.momth,
                day = this.day,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.year = source;
            if (target != 0 && target != int.MinValue) action.momth = target;
            if (value > 0) action.day = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
