// ChangeHomeEquipCharacter
using System;
using UnityEngine;

public class ChangeHomeEquipCharacterNode : ActionNode
{
        public int equipInstanceId;
        public int newPlayer;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeHomeEquipCharacter
        {
                equipInstanceId = this.equipInstanceId,
                newPlayer = this.newPlayer,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
