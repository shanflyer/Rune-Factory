// DisplayCharacterItemRenderer
using System;
using UnityEngine;

[Serializable]
public class DisplayCharacterItemRendererNode : ActionNode
{
        public int characterId;
        public int itemId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DisplayCharacterItemRenderer
        {
                characterId = this.characterId,
                itemId = this.itemId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.itemId = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
