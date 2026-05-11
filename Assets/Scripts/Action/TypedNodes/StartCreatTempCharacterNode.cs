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
            if (source != 0 && source != int.MinValue) action.creatDataId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}