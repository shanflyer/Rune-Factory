// CheckCharacterItemValue
using System;
using UnityEngine;

[Serializable]
public class CheckCharacterItemValueNode : ActionNode
{
        public int characterId;
        public int itemId;
        public int itemValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CheckCharacterItemValue
        {
                characterId = this.characterId,
                itemId = this.itemId,
                itemValue = this.itemValue,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.itemId = target;
            if (value != -1) action.itemValue = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
