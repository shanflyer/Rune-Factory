// CreatPackage
using System;
using UnityEngine;

public class CreatPackageNode : ActionNode
{
        public int packageDataId;
        public int level;
        public int instanceId;
        public bool playerPackage;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatPackage
        {
                packageDataId = this.packageDataId,
                level = this.level,
                instanceId = this.instanceId,
                playerPackage = this.playerPackage,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.instanceId = source;
            if (target != 0 && target != int.MinValue) action.packageDataId = target;
            if (value != -1) action.level = value;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}