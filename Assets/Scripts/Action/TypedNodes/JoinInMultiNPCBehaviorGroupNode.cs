// JoinInMultiNPCBehaviorGroup
using System;
using UnityEngine;

public class JoinInMultiNPCBehaviorGroupNode : ActionNode
{
        public int characterId;
        public int groupId;
        public bool faceCenter;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new JoinInMultiNPCBehaviorGroup
        {
                characterId = this.characterId,
                groupId = this.groupId,
                faceCenter = this.faceCenter,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.groupId = target;
            if (value != 0 && value != int.MinValue) action.faceCenter = value == 1;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
