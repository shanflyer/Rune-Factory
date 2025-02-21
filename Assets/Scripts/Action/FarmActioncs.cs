using System.Collections.Generic;
using Unity.Mathematics;

public struct RefreshField : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int fieldId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            fieldId = int.Parse(parameters[0].value);
        }
        if (source != 0)
        {
            fieldId = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshPlant : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int mapId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            mapId = int.Parse(parameters[0].value);
        }
        if (source != 0)
        {
            mapId = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TrySicklePlant : GameAction
{
    public int fieldId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            fieldId = source;
        }
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryGetPlantFruit : GameAction
{
    public int fieldId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            fieldId = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetWaterField : GameAction
{
    public int fieldId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryCreatPlant : GameAction
{
    public int fieldId;
    public int plantId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TrySmoothField : GameAction
{
    public int fieldId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct CheckPlant : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int instanceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
       
        if (source != 0)
        {
            instanceId = source;
        }
        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct CheckFieldState : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int instanceid;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        
        if (source != 0)
        {
            instanceid = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryCreatField : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int roomId;
    public int itemInstanceId;
    public int2 coordinate;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            roomId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            itemInstanceId = int.Parse(parameters[1].value);
        }
        if (source != 0)
        {
            roomId = source;
        }
        if (target != 0)
        {
            itemInstanceId = target;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}