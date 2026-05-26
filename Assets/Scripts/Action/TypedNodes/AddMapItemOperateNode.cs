// AddMapItemOperate
using System;
using UnityEngine;

[Serializable]
public class AddMapItemOperateNode : ActionNode
{
        public int mapItemId;
        public int addeOperateId;
        public bool needSave;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new AddMapItemOperate
        {
                mapItemId = this.mapItemId,
                addeOperateId = this.addeOperateId,
                needSave = this.needSave,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.mapItemId = source;
            if (target != 0 && target != int.MinValue) action.addeOperateId = target;
            if (value > 0) action.needSave = value == 1;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
