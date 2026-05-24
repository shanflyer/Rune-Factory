using System.Collections.Generic;

public struct PlayFishWater : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int fisherId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            fisherId = source;
        }
        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryGetFish : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterInstance;
    public int itemInstance;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            characterInstance = source;
        }
        if (target != 0 && target != int.MinValue)
        {
            itemInstance = target;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RecycleFisher : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterInstance;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            characterInstance = source;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct CreatFisher : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterInstance;
    public FishPondData pondData;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            characterInstance = source;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StopFishing : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            characterId = int.Parse(parameters[0].value);
        }
        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct NPCFishingResult : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public bool success;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[0].value);
            success = int.Parse(parameters[1].value) == 1;
        }
        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StartFishing : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int  mapItemId, characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 2)
        {
            mapItemId = int.Parse(parameters[0].value);
            characterId = int.Parse(parameters[1].value);
        }
        if (source != 0 && source != int.MinValue)
        {
            characterId = source;
        }
        if (target != 0 && target != int.MinValue)
        {
            mapItemId = target;
        }

        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StartFishingGame : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public int characterId;
    public int itemInstanceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setValue = setValue;
        this.setResult = setResult;
        if (source != 0 && source != int.MinValue)
        {
            characterId = source;
        }
        if (target != 0 && target != int.MinValue)
        {
            itemInstanceId = target;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct FishingIsSuccess : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public bool isSuccess;
    public FishPondData pondData;
    public int characterId;
    public float fishValue;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
