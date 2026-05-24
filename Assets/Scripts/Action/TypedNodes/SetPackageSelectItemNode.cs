// SetPackageSelectItem
using System;
using UnityEngine;

public class SetPackageSelectItemNode : ActionNode
{
        public int packageId;
        public int selectItem;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetPackageSelectItem
        {
                packageId = this.packageId,
                selectItem = this.selectItem,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
