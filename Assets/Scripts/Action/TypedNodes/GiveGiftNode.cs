// GiveGift
using System;
using UnityEngine;

public class GiveGiftNode : ActionNode
{
        public int giftId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new GiveGift
        {
                giftId = this.giftId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (value != -1) action.giftId = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}