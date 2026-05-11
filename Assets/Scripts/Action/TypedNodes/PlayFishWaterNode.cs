// PlayFishWater
using System;
using UnityEngine;

public class PlayFishWaterNode : ActionNode
{
        public int fisherId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new PlayFishWater
        {
                fisherId = this.fisherId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.fisherId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}