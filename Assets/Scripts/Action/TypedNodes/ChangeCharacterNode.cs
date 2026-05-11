// ChangeCharacter
using System;
using UnityEngine;

public class ChangeCharacterNode : ActionNode
{
        public int instanceId;
        public int newDataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeCharacter
        {
                instanceId = this.instanceId,
                newDataId = this.newDataId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}