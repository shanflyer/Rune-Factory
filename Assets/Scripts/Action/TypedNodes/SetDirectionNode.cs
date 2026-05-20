// SetDirection
using System;
using Unity.Mathematics;
using UnityEngine;

public class SetDirectionNode : ActionNode
{
        public int characterId;
        public float2 direction;
        public Direction directionEnum;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetDirection
        {
                characterId = this.characterId,
                direction = this.direction,
                directionEnum = this.directionEnum,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
