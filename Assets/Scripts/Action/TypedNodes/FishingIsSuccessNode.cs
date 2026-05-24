// FishingIsSuccess
using System;
using UnityEngine;

public class FishingIsSuccessNode : ActionNode
{
        public bool isSuccess;
        public int characterId;
        public float fishValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new FishingIsSuccess
        {
                isSuccess = this.isSuccess,
                characterId = this.characterId,
                fishValue = this.fishValue,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
