// SetMapEditorItemLinkCharacter
using System;
using UnityEngine;

[Serializable]
public class SetMapEditorItemLinkCharacterNode : ActionNode
{
        public int mapId;
        public int mapItemEditorId;
        public int linkInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetMapEditorItemLinkCharacter
        {
                mapId = this.mapId,
                mapItemEditorId = this.mapItemEditorId,
                linkInstanceId = this.linkInstanceId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != int.MinValue && source != 0) action.mapId = source;
            if (target != int.MinValue && target != 0) action.mapItemEditorId = target;
            if (value != int.MinValue && value != 0) action.linkInstanceId = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
