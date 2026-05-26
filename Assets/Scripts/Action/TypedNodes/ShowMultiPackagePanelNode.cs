// ShowMultiPackagePanel
using System;
using UnityEngine;

[Serializable]
public class ShowMultiPackagePanelNode : ActionNode
{
        public int packageId0;
        public int packageId1;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ShowMultiPackagePanel
        {
                packageId0 = this.packageId0,
                packageId1 = this.packageId1,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (action.packageId0 == 0 && action.packageId0 != int.MinValue)
                action.packageId0 = source == 0 ? CharacterManager.instance.controllerCharacter.characterPackage : source;
            if (action.packageId1 == 0 && action.packageId1 != int.MinValue)
                action.packageId1 = target == 0 ? CharacterManager.instance.controllerCharacter.characterPackage : target;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
