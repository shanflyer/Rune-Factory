// ShowEmote
using System;
using UnityEngine;

[Serializable]
public class ShowEmoteNode : ActionNode
{
        public int id;
        public EntityType entityType;
        public int emoteId;
        public int showTime;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ShowEmote
        {
                id = this.id,
                entityType = this.entityType,
                emoteId = this.emoteId,
                showTime = this.showTime,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
