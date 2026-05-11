// SetFilmUI
using System;
using UnityEngine;

public class SetFilmUINode : ActionNode
{
        public bool display;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetFilmUI
        {
                display = this.display,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}