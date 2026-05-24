// CreatControllerTempMapItem
using System;
using UnityEngine;

public class CreatControllerTempMapItemNode : ActionNode
{
        public int instanceId;
        public int dataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatControllerTempMapItem
        {
                instanceId = this.instanceId,
                dataId = this.dataId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
