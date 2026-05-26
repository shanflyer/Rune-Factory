// ShowItemAction
using System;
using UnityEngine;

[Serializable]
public class ShowItemActionNode : ActionNode
{
        public int itemId;
        public int mapInstanceId;
        public Vector3 position;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ShowItemAction
        {
                itemId = this.itemId,
                mapInstanceId = this.mapInstanceId,
                position = this.position,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
