// DisplayHurt
using System;
using UnityEngine;

[Serializable]
public class DisplayHurtNode : ActionNode
{
        public int targetId;
        public string hurtValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DisplayHurt
        {
                targetId = this.targetId,
                hurtValue = this.hurtValue,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
