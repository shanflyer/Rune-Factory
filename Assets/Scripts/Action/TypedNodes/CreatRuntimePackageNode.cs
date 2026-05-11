// CreatRuntimePackage
using System;
using UnityEngine;

public class CreatRuntimePackageNode : ActionNode
{
        public new string name;
        public int instanceId;
        public int caseCount;
        public bool itemPackage;
        public Vector2Int key;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatRuntimePackage
        {
                name = this.name,
                instanceId = this.instanceId,
                caseCount = this.caseCount,
                itemPackage = this.itemPackage,
                key = this.key,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}