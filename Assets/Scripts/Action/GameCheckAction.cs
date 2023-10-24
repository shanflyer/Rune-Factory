using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct CheckItemValue : GameAction
{
    public int packageId;
    public int itemDataId; 
    public int itemValue;
    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count >= 3)
        {
            packageId = int.Parse(parameters[0].value);
            itemDataId = int.Parse(parameters[1].value);
            itemValue = int.Parse(parameters[2].value);
        }
        if (source != 0)
            packageId = source;
        if (target != 0)
            itemDataId = target;
        if (value != 0)
            itemValue = value;
        GameActionManager.instance.QueueAction(this);
    }
}

public struct GameCheckAction : GameAction
{
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct CheckCharacterTemp : GameAction
{
    public int characterId;
    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}