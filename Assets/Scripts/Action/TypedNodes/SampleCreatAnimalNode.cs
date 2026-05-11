// SampleCreatAnimal
using System;
using UnityEngine;

public class SampleCreatAnimalNode : ActionNode
{
        public int dataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SampleCreatAnimal
        {
                dataId = this.dataId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.dataId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}