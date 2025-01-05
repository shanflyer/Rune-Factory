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
    public bool displaySunlight;
    public int skyId;
    public Vector2 startPos, endPos;
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 1)
        {
            display = bool.Parse(parameters[0].value);
        }
        if (parameters.Count >= 2)
        {
            displaySunlight = bool.Parse(parameters[1].value);
        }
        if (parameters.Count >= 3)
        {
            skyId = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct LerpScreenCycleValue : GameAction
{
    public float minCycleValue,maxCycleValue,lerpTime;
    public Vector2 cyclePos;
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 5)
        {
            minCycleValue = float.Parse(parameters[0].value);
            maxCycleValue = float.Parse(parameters[1].value);
            lerpTime = float.Parse(parameters[2].value);

            cyclePos = new Vector2(float.Parse(parameters[3].value), 
                float.Parse(parameters[4].value));
        }
        else if (parameters.Count >= 4)
        {
            minCycleValue = float.Parse(parameters[0].value);
            maxCycleValue = float.Parse(parameters[1].value);
            lerpTime = float.Parse(parameters[2].value);
            cyclePos = GameCommon.StringToVector3(parameters[3].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }

}