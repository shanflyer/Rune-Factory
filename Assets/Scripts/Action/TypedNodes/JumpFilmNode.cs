// JumpFilm
using System;
using UnityEngine;

public class JumpFilmNode : ActionNode
{
        public string filmName;
        public float jumpTime;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new JumpFilm
        {
                filmName = this.filmName,
                jumpTime = this.jumpTime,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
