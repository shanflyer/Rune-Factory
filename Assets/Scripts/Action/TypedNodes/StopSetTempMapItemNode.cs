// StopSetTempMapItem
using System;
using UnityEngine;

[Serializable]
public class StopSetTempMapItemNode : ActionNode
{
        public int instanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new StopSetTempMapItem
        {
                instanceId = this.instanceId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (target != 0 && target != int.MinValue) action.instanceId = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
