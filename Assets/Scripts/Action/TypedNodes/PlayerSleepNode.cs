// PlayerSleep
using System;
using UnityEngine;

public class PlayerSleepNode : ActionNode
{
        public int characterId;
        public int targetHour;
        public int targetMinute;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new PlayerSleep
        {
                characterId = this.characterId,
                targetHour = this.targetHour,
                targetMinute = this.targetMinute,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != int.MinValue) action.targetHour = target;
            if (value != int.MinValue) action.targetMinute = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
