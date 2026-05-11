// TryGetAnimalFoodFromPasture
using System;
using UnityEngine;

public class TryGetAnimalFoodFromPastureNode : ActionNode
{
        public int pastureId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryGetAnimalFoodFromPasture
        {
                pastureId = this.pastureId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}