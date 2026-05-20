// DisplayOrHideCharacter
using System;
using UnityEngine;

public class DisplayOrHideCharacterNode : ActionNode
{
        public int characterId;
        public bool display;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DisplayOrHideCharacter
        {
                characterId = this.characterId,
                display = this.display,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0) action.characterId = source;
            action.display = target == 1;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
