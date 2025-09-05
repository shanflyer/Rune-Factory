using System.Collections.Generic;

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
        this.setResult = setResult;
        this.setValue=setValue;
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
        this.setResult = setResult;
        this.setValue=setValue;
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
    public int characterId;
    public int fieldId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            fieldId = source;
        }

        if (target != 0 && target != int.MinValue) characterId = target;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetWaterField : GameAction
{
    public int fieldId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
    
}

public struct TryCreatPlant : GameAction
{
    public int fieldId;
    public int plantId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
    
}

public struct TrySmoothField : GameAction
{
    public int fieldId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
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
    public bool plantDeath;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        
        if (source != 0)
        {
            instanceid = source;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryCreateField : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public void Clear() { this = default; }
    public int roomId;
    public int itemInstanceId;
    public int editorInstanceId; 
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
       
        if (parameters.Count > 0)
        {
            itemInstanceId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            editorInstanceId = int.Parse(parameters[1].value);
        }
        if (parameters.Count > 2)
        {
            roomId = int.Parse(parameters[2].value);
        }
        if (source != 0)
        {
            itemInstanceId  = source;
            editorInstanceId = target;
            roomId = value;
        }
        
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}