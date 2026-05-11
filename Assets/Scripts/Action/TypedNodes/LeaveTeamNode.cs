// LeaveTeam
using System;
using UnityEngine;

public class LeaveTeamNode : ActionNode
{
        public int teamCharacterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new LeaveTeam
        {
                teamCharacterId = this.teamCharacterId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.teamCharacterId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}