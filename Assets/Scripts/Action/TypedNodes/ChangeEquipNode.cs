// ChangeEquip
using System;
using UnityEngine;

[Serializable]
public class ChangeEquipNode : ActionNode
{
        public int characterId;
        public int outPackageId;
        public int itemId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeEquip
        {
                characterId = this.characterId,
                outPackageId = this.outPackageId,
                itemId = this.itemId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0) action.characterId = source;
            if (target != 0) action.outPackageId = target;
            if (value != 0) action.itemId = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
