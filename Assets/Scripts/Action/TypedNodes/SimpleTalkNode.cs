// SimpleTalk
using System;
using UnityEngine;

[Serializable]
public class SimpleTalkNode : ActionNode
{
        public int talkId;
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SimpleTalk
        {
                talkId = this.talkId,
                characterId = this.characterId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (target != int.MinValue) action.talkId = target;
            if (source != int.MinValue) action.characterId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
