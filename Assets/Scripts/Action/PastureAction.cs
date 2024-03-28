using System.Collections.Generic;
using Unity.Mathematics;

public struct GetPastureLevel : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int pastureId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryUpPastureLevel : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int pastureId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryDeleteAnimal : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int animalId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct GetAnimalOutFromPasture : GameAction
{
    public int pastureId;
    public int animalId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetAnimalToPasture : GameAction
{
    public int pastureId;
    public int animalId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryCreatAnimal : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int roomId;
    public int2 coordinate;
    public int dataId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct AnimalCostFood : GameAction
{
    public int pastureId;
    public int animalDataId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
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

        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryGetAnimalFoodFromPasture : GameAction
{
    public int pastureId;
    public Item item;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TrySetAnimalFoodToPasture : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int pastureId;
    public Item item;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryGetItemFromPastureBox : GameAction
{
    public int pastureId;
    public Item item;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TrySetItemToPastureBox : GameAction
{
    public int pastureId;
    public Item item;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
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
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryCreatPasture : GameAction
{
    public int roomId;
    public int itemInstanceId;
    public int dataId;
    public string pastureName;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
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
            dataId = value;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryDeletePasture : GameAction
{
    public int instanceId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            instanceId = int.Parse(parameters[0].value);
        }

        if (source != 0)
        {
            instanceId = source;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}