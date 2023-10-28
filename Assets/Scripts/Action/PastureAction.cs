using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

public struct GetPastureNextLevelCost : GameAction
{
    public int pastureId;
    public SetValue setValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct GetPastureLevel : GameAction
{
    public int pastureId;
    public SetValue setValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TryUpPastureLevel : GameAction
{
    public int pastureId;
    public SetValue setValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TryDeleteAnimal : GameAction
{
    public int animalId;
    public SetValue setValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct GetAnimalOutFromPasture : GameAction
{
    public int pastureId;
    public int animalId;
    public SetResult setValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct SetAnimalToPasture : GameAction
{
    public int pastureId;
    public int animalId;
    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TryCreatAnimal : GameAction
{
    public int roomId;
    public int2 coordinate;
    public int dataId;
    public SetValue setValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        GameActionManager.instance.QueueAction(this);
    }
}
public struct AnimalCostFood : GameAction
{ 
    public int pastureId;
    public int animalDataId;
    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            pastureId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            animalDataId = int.Parse(parameters[1].value);
        } 
        if (source != 0)
        {
            pastureId = source;
        }
        if (target != 0)
        {
            animalDataId = target;
        }
        
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TryGetAnimalFoodFromPasture : GameAction
{
    public int pastureId;
    public Item item;
    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            pastureId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            item.value = int.Parse(parameters[1].value);
        }
        if (parameters.Count > 2)
        {
            item.count = int.Parse(parameters[2].value);
        }
        if (source != 0)
        {
            pastureId = source;
        }
        if (target != 0)
        {
            item.value = target;
        }
        if (value != 0)
        {
            item.count = value;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TrySetAnimalFoodToPasture : GameAction
{
    public int pastureId;
    public Item item;
    public SetValue setValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            pastureId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            item.value = int.Parse(parameters[1].value);
        }
        if (parameters.Count > 2)
        {
            item.count = int.Parse(parameters[2].value);
        }
        if (source != 0)
        {
            pastureId = source;
        }
        if (target != 0)
        {
            item.value = target;
        }
        if (value != 0)
        {
            item.count = value;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TryGetItemFromPastureBox : GameAction
{
    public int pastureId;
    public Item item;
    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            pastureId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            item.value = int.Parse(parameters[1].value);
        }
        if (parameters.Count > 2)
        {
            item.count = int.Parse(parameters[2].value);
        }
        if (source != 0)
        {
            pastureId = source;
        }
        if (target != 0)
        {
            item.value = target;
        }
        if (value != 0)
        {
            item.count = value;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TrySetItemToPastureBox : GameAction
{
    public int pastureId;
    public Item item;
    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    {
        if (parameters.Count > 0)
        {
            pastureId= int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            item.value= int.Parse(parameters[1].value);
        }
        if (parameters.Count > 2)
        {
            item.count = int.Parse(parameters[2].value);
        }
        if (source != 0)
        {
            pastureId = source;
        }
        if (target != 0)
        {
            item.value = target;
        }
        if (value != 0)
        {
            item.count = value;
        }
        GameActionManager.instance.QueueAction(this);
    }
}
public struct TryCreatPasture : GameAction
{
    public int roomId;
    public int itemInstanceId;
    public int linkItemDataId;
    public string pastureName;
    public SetResult setResult;
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
        if (value != 0)
        {
            linkItemDataId = value;
        }
        GameActionManager.instance.QueueAction(this);
    }
}

public struct TryDeletePasture : GameAction
{
    public int instanceId;
    public SetResult setResult;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1)
    { 
        if (parameters.Count > 0)
        {
            instanceId = int.Parse(parameters[0].value);
        }
         
        if (source != 0)
        {
            instanceId = source;
        } 
        GameActionManager.instance.QueueAction(this);
    }
}