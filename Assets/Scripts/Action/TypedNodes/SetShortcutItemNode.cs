// SetShortcutItem
using System;
using UnityEngine;

[Serializable]
public class SetShortcutItemNode : ActionNode
{
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetShortcutItem
        {
                characterId = this.characterId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
