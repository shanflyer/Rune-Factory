// OpenOrCloseInputMap
using System;
using UnityEngine;

[Serializable]
public class OpenOrCloseInputMapNode : ActionNode
{
        public bool open;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new OpenOrCloseInputMap
        {
                open = this.open,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            action.open = source == 1;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
