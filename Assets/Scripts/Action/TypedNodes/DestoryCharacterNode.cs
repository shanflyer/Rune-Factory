// DestoryCharacter
using System;
using UnityEngine;

public class DestoryCharacterNode : ActionNode
{
        public int characterId;
        public int dataId;
        public bool isTemp;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DestoryCharacter
        {
                characterId = this.characterId,
                dataId = this.dataId,
                isTemp = this.isTemp,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}