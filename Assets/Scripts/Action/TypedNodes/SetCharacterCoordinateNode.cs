// SetCharacterCoordinate
using System;
using UnityEngine;

public class SetCharacterCoordinateNode : ActionNode
{
        public int characterId;
        public bool fiexedDisplay;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetCharacterCoordinate
        {
                characterId = this.characterId,
                fiexedDisplay = this.fiexedDisplay,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}