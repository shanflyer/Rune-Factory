// CheckMapEditorItemLinkCharacter
using System;
using UnityEngine;

public class CheckMapEditorItemLinkCharacterNode : ActionNode
{
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CheckMapEditorItemLinkCharacter
        {
                characterId = this.characterId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (value != -1) action.characterId = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}