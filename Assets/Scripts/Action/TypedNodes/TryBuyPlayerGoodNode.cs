// TryBuyPlayerGood
using System;
using UnityEngine;

public class TryBuyPlayerGoodNode : ActionNode
{
        public int characterId;
        public int storeCounterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryBuyPlayerGood
        {
                characterId = this.characterId,
                storeCounterId = this.storeCounterId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}