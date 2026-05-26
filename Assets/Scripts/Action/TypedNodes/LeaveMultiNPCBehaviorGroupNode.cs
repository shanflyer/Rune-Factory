// LeaveMultiNPCBehaviorGroup
using System;
using UnityEngine;

[Serializable]
public class LeaveMultiNPCBehaviorGroupNode : ActionNode
{
        public int characterId;
        public int groupId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new LeaveMultiNPCBehaviorGroup
        {
                characterId = this.characterId,
                groupId = this.groupId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
