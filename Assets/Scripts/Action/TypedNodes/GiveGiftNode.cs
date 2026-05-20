// GiveGift
using System;
using UnityEngine;

public class GiveGiftNode : ActionNode
{
        public int giveCharacter;
        public int receiveCharacter;
        public int giftId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new GiveGift
        {
                giveCharacter = this.giveCharacter,
                receiveCharacter = this.receiveCharacter,
                giftId = this.giftId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0) action.giveCharacter = source;
            if (target != 0) action.receiveCharacter = target;
            if (value != 0) action.giftId = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
