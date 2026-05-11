// SetMapOverrideEnvironment
using System;
using UnityEngine;

public class SetMapOverrideEnvironmentNode : ActionNode
{
        public string dayEnvironmentDataName;
        public string duskEnvironmentDataName;
        public string dawnEnvironmentDataName;
        public string nightEnvironmentDataName;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetMapOverrideEnvironment
        {
                dayEnvironmentDataName = this.dayEnvironmentDataName,
                duskEnvironmentDataName = this.duskEnvironmentDataName,
                dawnEnvironmentDataName = this.dawnEnvironmentDataName,
                nightEnvironmentDataName = this.nightEnvironmentDataName,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}