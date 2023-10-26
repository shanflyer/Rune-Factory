using System;
using System.Collections.Generic;
using System.Linq;

public struct TryCreatPasture : GameAction
{
    public int roomId;
    public int itemInstanceId;
    public string pastureName;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            roomId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            itemInstanceId = int.Parse(parameters[1].value);
        }
        if (parameters.Count > 2)
        {
            pastureName = parameters[2].value;
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