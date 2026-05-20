// ChangeCharacterProperty
using System;
using UnityEngine;

public class ChangeCharacterPropertyNode : ActionNode
{
        public int characterId;
        public CharacterPropertyType propertyType;
        public int changeValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeCharacterProperty
        {
                characterId = this.characterId,
                propertyType = this.propertyType,
                changeValue = this.changeValue,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
