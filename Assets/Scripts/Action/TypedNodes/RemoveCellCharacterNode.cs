// RemoveCellCharacter
using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class RemoveCellCharacterNode : ActionNode
{
        public int3 cell;
        public int characterId;
        public bool isTemp;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemoveCellCharacter
        {
                characterId = this.characterId,
                cell = this.cell,
                isTemp = this.isTemp,
        };
        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
