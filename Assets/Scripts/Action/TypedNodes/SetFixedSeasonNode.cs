// SetFixedSeason
using System;
using UnityEngine;

public class SetFixedSeasonNode : ActionNode
{
        public float season;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetFixedSeason
        {
                season = this.season,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
