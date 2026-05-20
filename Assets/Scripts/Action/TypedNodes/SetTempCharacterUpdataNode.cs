// SetTempCharacterUpdata
using System;
using UnityEngine;

public class SetTempCharacterUpdataNode : ActionNode
{
        public bool canUpdata;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetTempCharacterUpdata
        {
                canUpdata = this.canUpdata,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            action.canUpdata = source == 1;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
