// RemovePackageItemInstance
using System;
using UnityEngine;

public class RemovePackageItemInstanceNode : ActionNode
{
        public int packageId;
        public int itemInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemovePackageItemInstance
        {
                packageId = this.packageId,
                itemInstanceId = this.itemInstanceId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}