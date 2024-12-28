using System.Collections.Generic;
using UnityEngine;

public struct PlayerWakeUp : GameAction
{
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        else
        {
            characterId = source;
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}

public struct PlayerSleep : GameAction
{
    public int characterId;
    public int targetHour, targetMinute;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            characterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            targetHour = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            targetMinute = int.Parse(parameters[2].value);
         
        if (source != 0 && source != int.MinValue)
        {
            characterId = source;
        }
        if (target != int.MinValue)
        {
            targetHour = target;
        }
        if (value != int.MinValue)
        {
            targetMinute = value;
        } 
        GameActionManager.instance.QueueAction(this, immediately);
    }

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}
public struct TimeRun : GameAction
{
    public bool run;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            run = bool.Parse(parameters[0].value);
        GameActionManager.instance.QueueAction(this, immediately);
    }
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}
public struct LerpGameTime : GameAction
{
    public int targetHour, targetMinute;
    public float totalTime;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            targetHour = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            targetMinute = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            totalTime = float.Parse(parameters[2].value);
        GameActionManager.instance.QueueAction(this, immediately);
    }

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}

public struct ClearOverrideEnvironment : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct WeatherAction : GameAction
{
    public int weatherDataId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetSeasonWeather : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetFixedTime : GameAction
{
    public int date;
    public int hour;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}
public struct SetFixedSeason : GameAction
{
    public float season;
    public WeatherDisplayType weatherDisplayType;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}
public struct SetMapOverrideEnvironment : GameAction
{
    public string dayEnvironmentDataName;
    public string duskEnvironmentDataName;
    public string dawnEnvironmentDataName;
    public string nightEnvironmentDataName;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            dayEnvironmentDataName = parameters[0].value;
        if (parameters.Count > 1)
            duskEnvironmentDataName = parameters[1].value;
        if (parameters.Count > 2)
            dawnEnvironmentDataName = parameters[2].value;
        if (parameters.Count > 3)
            nightEnvironmentDataName = parameters[3].value;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CreatWeather : GameAction
{
    public List<int> nowWeathers, nextWeather;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}
public struct SetWeather : GameAction
{
    public Weather weather;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}
public struct SetEnvironmentLight : GameAction
{
    public EnvironmentLightData environmentLightData;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}

public struct OverrideEnvironmentLight : GameAction
{
    public bool overSkyAndSun;
    public EnvironmentLightData environmentLightData;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}

public struct ClearOverrideEnvironmentLight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}