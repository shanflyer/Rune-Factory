// ShowItemResult
using System;
using UnityEngine;

public class ShowItemResultNode : ActionNode
{
        public int item;
        public string info;
        public int action;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ShowItemResult
        {
                item = this.item,
                info = this.info,
                action = this.action,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}