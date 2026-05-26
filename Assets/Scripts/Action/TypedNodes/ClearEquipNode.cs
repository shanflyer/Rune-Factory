// ClearEquip
using System;
using UnityEngine;

[Serializable]
public class ClearEquipNode : ActionNode
{
        public int characterId;
        public int outPackageId;
        public ItemType itemType;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ClearEquip
        {
                characterId = this.characterId,
                outPackageId = this.outPackageId,
                itemType = this.itemType,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0) action.characterId = source;
            if (target != 0) action.outPackageId = target;
            if (value != 0) action.itemType = (ItemType)value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
