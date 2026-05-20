// RemoveMapItemOperate
using System;
using UnityEngine;

public class RemoveMapItemOperateNode : ActionNode
{
        public int mapItemId;
        public int removeOperateId;
        public bool needSave;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemoveMapItemOperate
        {
                mapItemId = this.mapItemId,
                removeOperateId = this.removeOperateId,
                needSave = this.needSave,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.mapItemId = source;
            if (target != 0 && target != int.MinValue) action.removeOperateId = target;
            if (value > 0) action.needSave = value == 1;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
