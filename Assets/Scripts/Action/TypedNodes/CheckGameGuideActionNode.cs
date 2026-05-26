// CheckGameGuideAction
using System;
using UnityEngine;

[Serializable]
public class CheckGameGuideActionNode : ActionNode
{
        public int guidKey;
        public bool isEnd;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CheckGameGuideAction
        {
                guidKey = this.guidKey,
                isEnd = this.isEnd,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.guidKey = source;
            action.isEnd = target != 0;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
