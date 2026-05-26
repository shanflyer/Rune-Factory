// TryGetItemFromPastureBox
using System;
using UnityEngine;

[Serializable]
public class TryGetItemFromPastureBoxNode : ActionNode
{
        public int pastureId;
        public Item item;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryGetItemFromPastureBox
        {
                pastureId = this.pastureId,
                item = this.item,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0) action.pastureId = source;
            if (target != 0) action.item = Item.SetValue(action.item, target).GetAwaiter().GetResult();
            if (value != 0) action.item.count = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
