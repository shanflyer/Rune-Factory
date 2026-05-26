// SetItemAnimation
using System;
using UnityEngine;

[Serializable]
public class SetItemAnimationNode : ActionNode
{
        public int mapId;
        public int id;
        public int editorId;
        public int keyX;
        public int keyY;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetItemAnimation
        {
                mapId = this.mapId,
                id = this.id,
                editorId = this.editorId,
                keyX = this.keyX,
                keyY = this.keyY,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.id = source;
            if (target != 0 && target != int.MinValue) action.keyX = target;
            if (value != 0 && value != int.MinValue) action.keyY = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
