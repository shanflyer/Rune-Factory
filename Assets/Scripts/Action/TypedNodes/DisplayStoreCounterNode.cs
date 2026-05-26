// DisplayStoreCounter
using System;
using UnityEngine;

[Serializable]
public class DisplayStoreCounterNode : ActionNode
{
        public bool display;
        public int itemInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DisplayStoreCounter
        {
                display = this.display,
                itemInstanceId = this.itemInstanceId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
