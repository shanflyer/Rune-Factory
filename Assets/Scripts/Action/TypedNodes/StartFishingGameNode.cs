// StartFishingGame
using System;
using UnityEngine;

[Serializable]
public class StartFishingGameNode : ActionNode
{
        public int characterId;
        public int itemInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new StartFishingGame
        {
                characterId = this.characterId,
                itemInstanceId = this.itemInstanceId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.itemInstanceId = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
