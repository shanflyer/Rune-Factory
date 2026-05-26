// SetChapterFight
using System;
using UnityEngine;

[Serializable]
public class SetChapterFightNode : ActionNode
{
        public bool isInFight;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetChapterFight
        {
                isInFight = this.isInFight,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
