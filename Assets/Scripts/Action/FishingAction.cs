using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct TryGetFish : GameAction
{
    public int fishPondId;
    public int characterId;

    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TryCreatFishPond : GameAction
{
    public int dataId;
    public int itemId;
    public SetValue setValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct TryDeleteFishPond : GameAction
{
    public int instanceId;
    public int itemId;
    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}