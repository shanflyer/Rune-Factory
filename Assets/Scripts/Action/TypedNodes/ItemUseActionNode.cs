// ItemUseAction
using System;
using UnityEngine;

[Serializable]
public class ItemUseActionNode : ActionNode
{
        public int packageId;
        public int itemId;
        public int itemCount;
        public int itemInstance;
        public int targetCharacter;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ItemUseAction
        {
                packageId = this.packageId,
                itemId = this.itemId,
                itemCount = this.itemCount,
                itemInstance = this.itemInstance,
                targetCharacter = this.targetCharacter,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
