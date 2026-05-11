// PauseFilm
using System;
using UnityEngine;

public class PauseFilmNode : ActionNode
{
        public string filmName;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new PauseFilm
        {
                filmName = this.filmName,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}