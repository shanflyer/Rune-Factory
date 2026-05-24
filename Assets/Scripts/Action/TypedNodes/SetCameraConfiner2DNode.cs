// SetCameraConfiner2D
using System;
using UnityEngine;

public class SetCameraConfiner2DNode : ActionNode
{
        public bool enable;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCameraConfiner2D
        {
                enable = this.enable,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
