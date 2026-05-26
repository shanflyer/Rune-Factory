// RefreshShopLevel
using System;
using UnityEngine;

[Serializable]
public class RefreshShopLevelNode : ActionNode
{
        public string shopName;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshShopLevel
        {
                shopName = this.shopName,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
