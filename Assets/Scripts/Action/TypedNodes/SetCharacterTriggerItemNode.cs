// SetCharacterTriggerItem
using System;
using Unity.Mathematics;
using UnityEngine;

public class SetCharacterTriggerItemNode : ActionNode
{
        public int characterId;
        public int2 mapItemEditId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCharacterTriggerItem
        {
                characterId = this.characterId,
                mapItemEditId = this.mapItemEditId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
