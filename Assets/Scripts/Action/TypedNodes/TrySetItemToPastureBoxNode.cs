// TrySetItemToPastureBox
using System;
using UnityEngine;

public class TrySetItemToPastureBoxNode : ActionNode
{
        public int pastureId;
        public int itemId;
        public int itemCount;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TrySetItemToPastureBox
        {
                pastureId = this.pastureId,
                itemId = this.itemId,
                itemCount = this.itemCount,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.pastureId = source;
            if (target != 0 && target != int.MinValue) action.itemId = target;
            if (value != -1) action.itemCount = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}