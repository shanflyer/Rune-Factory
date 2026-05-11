// CreatTempCharacter
using System;
using UnityEngine;

public class CreatTempCharacterNode : ActionNode
{
        public int characterId;
        public int mapInstance;
        public int coordinateX;
        public int coordinateY;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatTempCharacter
        {
                characterId = this.characterId,
                mapInstance = this.mapInstance,
                coordinateX = this.coordinateX,
                coordinateY = this.coordinateY,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}