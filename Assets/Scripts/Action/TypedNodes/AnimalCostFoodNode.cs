// AnimalCostFood
using System;
using UnityEngine;

public class AnimalCostFoodNode : ActionNode
{
        public int pastureId;
        public int animalDataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new AnimalCostFood
        {
                pastureId = this.pastureId,
                animalDataId = this.animalDataId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.pastureId = source;
            if (target != 0 && target != int.MinValue) action.animalDataId = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}