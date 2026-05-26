// SetCharacterAnimator
using System;
using UnityEngine;

[Serializable]
public class SetCharacterAnimatorNode : ActionNode
{
        public int characterId;
        public string parameter;
        public ParameterType parameterType;
        public bool boolValue;
        public int intValue;
        public float floatValue;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCharacterAnimator
        {
                characterId = this.characterId,
                parameter = this.parameter,
                parameterType = this.parameterType,
                boolValue = this.boolValue,
                intValue = this.intValue,
                floatValue = this.floatValue,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
