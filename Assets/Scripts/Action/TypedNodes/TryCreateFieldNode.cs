// TryCreateField
using System;
using UnityEngine;

[Serializable]
public class TryCreateFieldNode : ActionNode
{
        public int itemInstanceId;
        public int editorInstanceId;
        public int roomId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryCreateField
        {
                itemInstanceId = this.itemInstanceId,
                editorInstanceId = this.editorInstanceId,
                roomId = this.roomId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0)
            {
                action.itemInstanceId = source;
                action.editorInstanceId = target;
                action.roomId = value;
            }
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
