// DisplaySky
using System;
using UnityEngine;

public class DisplaySkyNode : ActionNode
{
        public bool display;
        public bool displaySunlight;
        public int skyId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new DisplaySky
        {
                display = this.display,
                displaySunlight = this.displaySunlight,
                skyId = this.skyId,
        };
        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
