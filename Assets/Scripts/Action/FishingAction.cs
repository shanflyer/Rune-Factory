using System.Collections.Generic;

public struct DestoryFishPond : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int PondId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct RefreshFishPondObj : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int pondId;
    public int room;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
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

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct RemoveFish : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int instanceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct TryGetFish : GameAction
{
    public int fishId;
    public int characterId;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct TryCreatFishPond : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int dataId;
    public int fishId;
    public int itemId;
    public int room;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct TryDeleteFishPond : GameAction
{
    public int instanceId;
    public int itemId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}