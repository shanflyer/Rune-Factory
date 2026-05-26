// SetMapItemLinkCharacter
using System;
using UnityEngine;

[Serializable]
public class SetMapItemLinkCharacterNode : ActionNode
{
        public int mapItemInstanceId;
        public int linkInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetMapItemLinkCharacter
        {
                mapItemInstanceId = this.mapItemInstanceId,
                linkInstanceId = this.linkInstanceId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.mapItemInstanceId = source;
            if (target != 0 && target != int.MinValue) action.linkInstanceId = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
