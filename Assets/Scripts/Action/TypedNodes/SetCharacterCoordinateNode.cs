// SetCharacterCoordinate
using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class SetCharacterCoordinateNode : ActionNode
{
        public int characterId;
        public int3 coordinate;
        public bool fiexedDisplay;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCharacterCoordinate
        {
                characterId = this.characterId,
                coordinate = this.coordinate,
                fiexedDisplay = this.fiexedDisplay,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
