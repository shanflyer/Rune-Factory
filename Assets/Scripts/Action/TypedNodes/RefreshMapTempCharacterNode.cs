// RefreshMapTempCharacter
using System;
using UnityEngine;

public class RefreshMapTempCharacterNode : ActionNode
{
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshMapTempCharacter
        {
                characterId = this.characterId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}