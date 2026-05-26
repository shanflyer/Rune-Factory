// CreatTeamPlayer
using System;
using UnityEngine;

[Serializable]
public class CreatTeamPlayerNode : ActionNode
{
        public bool holdDisplay;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatTeamPlayer
        {
                holdDisplay = this.holdDisplay,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
