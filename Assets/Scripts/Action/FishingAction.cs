using System.Collections.Generic;
using Unity.Mathematics;
 
public struct PlayFishWater : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
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
public struct UnLinkFisher : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int fishId;
    public int fisherId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            fishId = source;
        }
        if (target != 0 && target != int.MinValue)
        {
            fisherId = target;
        }
        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TrueLinkFisher : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int fishId;
    public int fisherId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            fishId = source;
        }
        if (target != 0 && target != int.MinValue)
        {
            fisherId = target;
        }
        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct LinkFisher : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int fishId;
    public bool trueLink;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            fishId = source;
        }
        if(target != 0 && target != int.MinValue) 
        {
            trueLink = target==1;
        }
        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
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
    public int characterId;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {

        if (source != 0 && source != int.MinValue)
        {
            characterId = source;
        }
        this.setValue = setValue;
        this.setResult = setResult;
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