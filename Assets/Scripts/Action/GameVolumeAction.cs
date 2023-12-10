using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TextCore.Text;

public struct DisplaySky : GameAction
{
    public bool display;
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count >= 1)
        {
            display = bool.Parse(parameters[0].value); 
        }

        GameActionManager.instance.QueueAction(this);
    }
}
public struct LerpScreenCycleValue : GameAction
{
    public float minCycleValue,maxCycleValue,lerpTime;
    public Vector2 cyclePos;
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count >= 5)
        {
            minCycleValue = float.Parse(parameters[0].value);
            maxCycleValue = float.Parse(parameters[1].value);
            lerpTime = float.Parse(parameters[2].value);

            cyclePos = new Vector2(float.Parse(parameters[3].value), 
                float.Parse(parameters[4].value));
        }
        GameActionManager.instance.QueueAction(this);
    }

}