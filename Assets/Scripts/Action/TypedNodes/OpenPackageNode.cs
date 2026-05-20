// OpenPackage
using System;
using UnityEngine;

public class OpenPackageNode : ActionNode
{
        public int packageId;
        public string selectActionName;
        public int selectActionId;
        public bool canSetShortcut;
        public bool eventAction;
        public int targetObj;
        public bool isMiniShow;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new OpenPackage
        {
                packageId = this.packageId,
                selectActionName = this.selectActionName,
                selectActionId = this.selectActionId,
                canSetShortcut = this.canSetShortcut,
                eventAction = this.eventAction,
                targetObj = this.targetObj,
                isMiniShow = this.isMiniShow,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.packageId = source;
            if (target != 0 && target != int.MinValue) action.canSetShortcut = target != 0;
            if (target != 0 && target != int.MinValue) action.targetObj = target;
            if (value != -1) action.selectActionId = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
