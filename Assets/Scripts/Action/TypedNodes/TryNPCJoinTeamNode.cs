// TryNPCJoinTeam
using System;
using UnityEngine;

public class TryNPCJoinTeamNode : ActionNode
{
        public int characterId;
        public int teamCharacterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryNPCJoinTeam
        {
                characterId = this.characterId,
                teamCharacterId = this.teamCharacterId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.teamCharacterId = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
