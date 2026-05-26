// OpenPanelAction
using System;
using UnityEngine;

[Serializable]
public class OpenPanelActionNode : ActionNode
{
        public string typeName;
        public string dataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new OpenPanelAction
        {
                type = Type.GetType(this.typeName),
                dataId = this.dataId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.dataId = source.ToString();
            if (target != 0 && target != int.MinValue) action.dataId = target.ToString();
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
