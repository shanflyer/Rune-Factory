// BuyPlayerGood
using System;
using UnityEngine;

public class BuyPlayerGoodNode : ActionNode
{
        public int storeCounterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new BuyPlayerGood
        {
                storeCounterId = this.storeCounterId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (target != 0 && target != int.MinValue) action.storeCounterId = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
