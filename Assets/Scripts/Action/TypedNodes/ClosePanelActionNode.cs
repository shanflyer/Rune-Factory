// container - ClosePanelAction
using System;
using UnityEngine;

public class ClosePanelActionNode : ActionNode
{
        public string typeName;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ClosePanelAction
        {
                type = Type.GetType(this.typeName),
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
