// SetAnimalToPasture
using System;
using UnityEngine;

[Serializable]
public class SetAnimalToPastureNode : ActionNode
{
        public int pastureId;
        public int animalId;
        public bool refreshPos;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetAnimalToPasture
        {
                pastureId = this.pastureId,
                animalId = this.animalId,
                refreshPos = this.refreshPos,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
