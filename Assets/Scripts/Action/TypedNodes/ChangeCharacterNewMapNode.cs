// ChangeCharacterNewMap
using System;
using UnityEngine;

[Serializable]
public class ChangeCharacterNewMapNode : ActionNode
{
        public int characterInstance;
        public int newMap;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeCharacterNewMap
        {
                characterInstance = this.characterInstance,
                newMap = this.newMap,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterInstance = source;
            if (target != 0 && target != int.MinValue) action.newMap = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
