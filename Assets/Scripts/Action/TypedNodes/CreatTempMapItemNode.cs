// CreatTempMapItem
using System;
using UnityEngine;

[Serializable]
public class CreatTempMapItemNode : ActionNode
{
        public int characterId;
        public int instanceId;
        public int dataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatTempMapItem
        {
                characterId = this.characterId,
                instanceId = this.instanceId,
                dataId = this.dataId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.instanceId = target;
            if (value != 0 && value != int.MinValue) action.dataId = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
