// AddBuffAction
using System;
using UnityEngine;

public class AddBuffActionNode : ActionNode
{
        public int characterId;
        public int buffDataId;
        public int overrideLifeTime;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new AddBuffAction
        {
                characterId = this.characterId,
                buffDataId = this.buffDataId,
                overrideLifeTime = this.overrideLifeTime,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
