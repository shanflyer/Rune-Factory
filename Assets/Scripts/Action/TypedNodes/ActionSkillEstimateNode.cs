// ActionSkillEstimate
using System;
using UnityEngine;

public class ActionSkillEstimateNode : ActionNode
{
        public int skillId;
        public int sourceId;
        public int targetId;
        public int index;
        public bool displayHurt;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ActionSkillEstimate
        {
                skillId = this.skillId,
                sourceId = this.sourceId,
                targetId = this.targetId,
                index = this.index,
                displayHurt = this.displayHurt,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
