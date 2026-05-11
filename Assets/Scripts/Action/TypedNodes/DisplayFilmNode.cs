// DisplayFilm
using System;
using UnityEngine;

public class DisplayFilmNode : ActionNode
{
        public string filmName;
        public string path;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DisplayFilm
        {
                filmName = this.filmName,
                path = this.path,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}