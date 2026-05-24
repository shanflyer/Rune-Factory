// container - TryStartAutoBehavior
using System;
using UnityEngine;

public class TryStartAutoBehaviorNode : ContainerNode
{
    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryStartAutoBehavior { setValue = setValue, setResult = setResult };
        ExecuteChildren(source, target, value, setResult, setValue, immediately);
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
