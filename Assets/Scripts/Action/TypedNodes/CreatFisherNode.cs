// CreatFisher
using System;
using UnityEngine;

public class CreatFisherNode : ActionNode
{
        public int characterInstance;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatFisher
        {
                characterInstance = this.characterInstance,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterInstance = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
