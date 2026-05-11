// CreatStoreCounter
using System;
using UnityEngine;

public class CreatStoreCounterNode : ActionNode
{
        public int itemInstanceId;
        public int storeDataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatStoreCounter
        {
                itemInstanceId = this.itemInstanceId,
                storeDataId = this.storeDataId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}