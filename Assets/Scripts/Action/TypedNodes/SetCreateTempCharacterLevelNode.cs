// SetCreateTempCharacterLevel
using System;
using UnityEngine;

[Serializable]
public class SetCreateTempCharacterLevelNode : ActionNode
{
        public int level;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCreateTempCharacterLevel
        {
                level = this.level,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (target != 0 && target != int.MinValue) action.level = target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
