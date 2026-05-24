// StartCharacterMove
using System;
using UnityEngine;

public class StartCharacterMoveNode : ActionNode
{
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new StartCharacterMove
        {
                characterId = this.characterId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (target != 0 && target != int.MinValue) action.characterId = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
