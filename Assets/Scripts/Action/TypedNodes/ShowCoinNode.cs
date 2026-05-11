// ShowCoin
using System;
using UnityEngine;

public class ShowCoinNode : ActionNode
{
        public Vector2 pos;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ShowCoin
        {
                pos = this.pos,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}