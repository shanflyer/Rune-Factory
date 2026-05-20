// StartCreatTempCharacter
using System;
using UnityEngine;

public class StartCreatTempCharacterNode : ActionNode
{
        public int creatDataId;
        public bool clearAll;
        public bool prewarm;
        public int overrideMaxCount;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new StartCreatTempCharacter
        {
                creatDataId = this.creatDataId,
                clearAll = this.clearAll,
                prewarm = this.prewarm,
                overrideMaxCount = this.overrideMaxCount,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source > 0) action.creatDataId = source;
            if (target >= 0) action.clearAll = target == 1;
            if (value >= 0) action.prewarm = value == 1;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
