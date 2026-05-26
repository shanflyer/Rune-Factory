// ChangeMapItemObjLayer
using System;
using UnityEngine;

[Serializable]
public class ChangeMapItemObjLayerNode : ActionNode
{
        public int mapItemId;
        public int layerId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeMapItemObjLayer
        {
                mapItemId = this.mapItemId,
                layerId = this.layerId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
