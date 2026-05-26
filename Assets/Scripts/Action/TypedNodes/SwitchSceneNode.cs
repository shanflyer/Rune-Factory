// SwitchScene
using System;
using UnityEngine;

[Serializable]
public class SwitchSceneNode : ActionNode
{
        public string sceneName;
        public int beforeLoadActionId;
        public int afterLoadActionId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SwitchScene
        {
                sceneName = this.sceneName,
                beforeLoadActionId = this.beforeLoadActionId,
                afterLoadActionId = this.afterLoadActionId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
