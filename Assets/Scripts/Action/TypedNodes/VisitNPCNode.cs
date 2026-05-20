// container - VisitNPC
using System;
using UnityEngine;

public class VisitNPCNode : ContainerNode
{
        public int targetId;
        public int sourceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new VisitNPC
        {
            targetId = this.targetId,
            sourceId = this.sourceId,
            setValue = setValue,
            setResult = setResult
        };
            if (source != 0) action.sourceId = source;
            if (target != 0) action.targetId = target;
        ExecuteChildren(source, target, value, setResult, setValue, immediately);
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
