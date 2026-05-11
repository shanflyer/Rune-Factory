// container - SetEnvironmentLight
using System;
using UnityEngine;

public class SetEnvironmentLightNode : ContainerNode
{
    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetEnvironmentLight { setValue = setValue, setResult = setResult };
        ExecuteChildren(source, target, value, setResult, setValue, immediately);
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}