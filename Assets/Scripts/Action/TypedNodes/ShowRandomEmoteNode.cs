// ShowRandomEmote
using System;
using UnityEngine;

public class ShowRandomEmoteNode : ActionNode
{
        public new int id;
        public EntityType entityType;
        public int randomId;
        public int showTime;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ShowRandomEmote
        {
                id = this.id,
                entityType = this.entityType,
                randomId = this.randomId,
                showTime = this.showTime,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
