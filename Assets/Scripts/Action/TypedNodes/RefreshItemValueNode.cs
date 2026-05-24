// RefreshItemValue
using System;
using UnityEngine;

public class RefreshItemValueNode : ActionNode
{
        public int characterId;
        public int itemId;
        public int itemValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshItemValue
        {
                characterId = this.characterId,
                itemId = this.itemId,
                itemValue = this.itemValue,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
