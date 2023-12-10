using System.Collections.Generic;

public struct RefreshField : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int fieldId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct RefreshPlant : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int plantId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct TryGetPlantFruit : GameAction
{
    public int fieldId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct SetWaterField : GameAction
{
    public int fieldId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct TryCreatPlant : GameAction
{
    public int fieldId;
    public int plantId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct TrySmoothField : GameAction
{
    public int fieldId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct CheckFieldState : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int instanceid;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
        if (source != 0)
        {
            instanceid = source;
        }
    }
}

public struct TryCreatField : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int roomId;
    public int itemInstanceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
        GameActionManager.instance.QueueAction(this);
    }
}