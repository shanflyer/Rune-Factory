// AddPackageItem
using System;
using UnityEngine;

[Serializable]
public class AddPackageItemNode : ActionNode
{
        public int packageId;
        public int itemDataId;
        public int itemCount;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new AddPackageItem
        {
                packageId = this.packageId,
                itemDataId = this.itemDataId,
                itemCount = this.itemCount,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
