// TryGetFish
using System;
using UnityEngine;

public class TryGetFishNode : ActionNode
{
        public int characterInstance;
        public int itemInstance;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryGetFish
        {
                characterInstance = this.characterInstance,
                itemInstance = this.itemInstance,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterInstance = source;
            if (target != 0 && target != int.MinValue) action.itemInstance = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}