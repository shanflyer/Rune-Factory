using System.Collections.Generic;
using Unity.Mathematics;

public struct CreatFisher : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterInstance;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            characterInstance = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RecycleFisher : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterInstance; 

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            characterInstance = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DestoryFishPond : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int PondId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshFishPondObj : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int pondId;
    public int room;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CreatFish : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int pondId;
    public int dataId;
    public int fishValue;
    public int room;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RemoveFish : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int instanceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryGetFish : GameAction
{
    public int fishId;
    public int characterId;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryCreatFishPond : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public int itemId;
    public int room;
    public int instanceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryDeleteFishPond : GameAction
{
    public int instanceId; 
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}