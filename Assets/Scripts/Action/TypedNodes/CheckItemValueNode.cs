// CheckItemValue
using System;
using UnityEngine;

public class CheckItemValueNode : ActionNode
{
        public int packageId;
        public int itemDataId;
        public int itemValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CheckItemValue
        {
                packageId = this.packageId,
                itemDataId = this.itemDataId,
                itemValue = this.itemValue,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.packageId = source;
            if (target != 0 && target != int.MinValue) action.itemDataId = target;
            if (value != -1) action.itemValue = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}