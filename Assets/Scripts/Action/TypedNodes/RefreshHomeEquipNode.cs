// RefreshHomeEquip
using System;
using UnityEngine;

public class RefreshHomeEquipNode : ActionNode
{
        public int equipInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshHomeEquip
        {
                equipInstanceId = this.equipInstanceId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
