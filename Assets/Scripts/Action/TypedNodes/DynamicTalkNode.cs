// DynamicTalk
using System;
using UnityEngine;

[Serializable]
public class DynamicTalkNode : ActionNode
{
        public string content;
        public string name;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DynamicTalk
        {
                content = this.content,
                name = this.name,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
