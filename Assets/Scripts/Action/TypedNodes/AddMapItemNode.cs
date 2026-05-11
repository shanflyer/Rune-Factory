// AddMapItem
using System;
using UnityEngine;

public class AddMapItemNode : ActionNode
{
        public int mapId;
        public int dataId;
        public int instanceId;
        public int fixeInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new AddMapItem
        {
                mapId = this.mapId,
                dataId = this.dataId,
                instanceId = this.instanceId,
                fixeInstanceId = this.fixeInstanceId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}