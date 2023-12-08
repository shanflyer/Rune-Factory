using System.Collections.Generic;
using UnityEngine.TextCore.Text;

public struct LerpGameTime : GameAction
{
    public int targetHour,targetMinute;
    public float totalTime;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null)
    {
        if (parameters.Count > 0)
            targetHour = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            targetMinute = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            totalTime = float.Parse(parameters[2].value);
        GameActionManager.instance.QueueAction(this);
    }
    public SetResult setResult { get; set; }
}
public struct ClearOverrideEnvironment : GameAction
{ 
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null)
    { 
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetMapOverrideEnvironment : GameAction
{
    public string dayEnvironmentDataName;
    public string duskEnvironmentDataName;
    public string dawnEnvironmentDataName;
    public string nightEnvironmentDataName;
    public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null)
    {
        if (parameters.Count > 0)
            dayEnvironmentDataName = parameters[0].value;
        if (parameters.Count > 1)
            duskEnvironmentDataName = parameters[1].value;
        if (parameters.Count > 2)
            dawnEnvironmentDataName = parameters[2].value;
        if (parameters.Count > 3)
            nightEnvironmentDataName = parameters[3].value;
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetEnvironmentLight : GameAction
{
    public EnvironmentLightData environmentLightData;
    public SetResult setResult { get; set; }
} 
public struct OverrideEnvironmentLight : GameAction
{
    public bool overSkyAndSun;
    public EnvironmentLightData environmentLightData;
    public SetResult setResult { get; set; }
}
public struct ClearOverrideEnvironmentLight : GameAction
{
     public SetResult setResult { get; set; }
}