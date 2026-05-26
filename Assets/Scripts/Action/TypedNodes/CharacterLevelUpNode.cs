// CharacterLevelUp
using System;
using UnityEngine;

[Serializable]
public class CharacterLevelUpNode : ActionNode
{
        public int characterId;
        public int level;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CharacterLevelUp
        {
                characterId = this.characterId,
                level = this.level,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
