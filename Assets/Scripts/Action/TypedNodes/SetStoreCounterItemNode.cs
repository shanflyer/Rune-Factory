// SetStoreCounterItem
using System;
using UnityEngine;

public class SetStoreCounterItemNode : ActionNode
{
        public int storeCounterId;
        public int itemId;
        public int count;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetStoreCounterItem
        {
                storeCounterId = this.storeCounterId,
                itemId = this.itemId,
                count = this.count,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.storeCounterId = source;
            if (target != 0 && target != int.MinValue) action.itemId = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
