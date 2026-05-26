// SwitchFunctionButton
using System;
using UnityEngine;

[Serializable]
public class SwitchFunctionButtonNode : ActionNode
{
        public bool fight;
        public bool auto;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SwitchFunctionButton
        {
                fight = this.fight,
                auto = this.auto,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
