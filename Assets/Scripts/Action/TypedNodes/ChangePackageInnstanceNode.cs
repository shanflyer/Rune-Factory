// ChangePackageInnstance
using System;
using UnityEngine;

[Serializable]
public class ChangePackageInnstanceNode : ActionNode
{
        public int oldInstanceId;
        public int newInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangePackageInnstance
        {
                oldInstanceId = this.oldInstanceId,
                newInstanceId = this.newInstanceId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.oldInstanceId = source;
            if (target != 0 && target != int.MinValue) action.newInstanceId = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
