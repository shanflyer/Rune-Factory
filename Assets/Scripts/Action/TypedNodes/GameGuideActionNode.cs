// GameGuideAction
using System;
using UnityEngine;

public class GameGuideActionNode : ActionNode
{
        public int guidKey;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new GameGuideAction
        {
                guidKey = this.guidKey,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.guidKey = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
