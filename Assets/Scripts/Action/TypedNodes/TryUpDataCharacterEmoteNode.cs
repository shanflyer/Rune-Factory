// TryUpDataCharacterEmote
using System;
using UnityEngine;

public class TryUpDataCharacterEmoteNode : ActionNode
{
        public new int id;
        public int emote;
        public int showTime;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryUpDataCharacterEmote
        {
                id = this.id,
                emote = this.emote,
                showTime = this.showTime,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}