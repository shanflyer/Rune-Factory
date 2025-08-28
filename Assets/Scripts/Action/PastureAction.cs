using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;

public struct GetPastureLevel : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int pastureId;
  
}

public struct TryUpPastureLevel : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int pastureId;
    public int itemInstance;
    public int roomId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            pastureId = int.Parse(parameters[0].value);
        }
        if (source != 0 && source != int.MinValue)
        {
            pastureId = source;
        }
        if (parameters.Count > 1)
        {
            itemInstance = int.Parse(parameters[1].value);
        }
        if (target != 0 && target != int.MinValue)
        {
            itemInstance = target;
        }
        if (parameters.Count > 2)
        {
            roomId = int.Parse(parameters[2].value);
        }
        if (value != 0 && value != int.MinValue)
        {
            roomId = value;
        }
        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryDeleteAnimal : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int animalId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    { 
        if (source != 0 && source != int.MinValue)
        {
            animalId = source;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
      
    }
}

public struct GetAnimalOutFromPasture : GameAction
{
    public int pastureId;
    public int animalId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

}

public struct RefreshAnimalPos : GameAction
{ 
    public int animalId;
    public bool refreshPos;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    
}
public struct SetAnimalToPasture : GameAction
{
    public int pastureId;
    public int animalId;
    public bool refreshPos;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

}
public struct SampleCreatAnimal : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public void Clear() { this = default; }  
    public int dataId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (source != 0 && source != int.MinValue)
        {
            dataId = source;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryCreatAnimal : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int roomId;
    public int2 coordinate;
    public int dataId;

}

public struct AnimalCostFood : GameAction
{
    public int pastureId;
    public int animalDataId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

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
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryGetAnimalFoodFromPasture : GameAction
{
    public int pastureId;
    public Item item;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public async void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            pastureId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            item=await Item.SetValue(item,int.Parse(parameters[1].value));
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
            item = await Item.SetValue(item, target);
        }
        if (value != 0)
        {
            item.count = value;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TrySetAnimalFoodToPasture : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int pastureId;
    public Item item;

    public async Task Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            pastureId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            item = await Item.SetValue(item, int.Parse(parameters[1].value));
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
            item = await Item.SetValue(item, target);
        }
        if (value != 0)
        {
            item.count = value;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryGetItemFromPastureBox : GameAction
{
    public int pastureId;
    public Item item;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public async void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            pastureId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            item = await Item.SetValue(item, int.Parse(parameters[1].value));
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
            item = await Item.SetValue(item, target);
        }
        if (value != 0)
        {
            item.count = value;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TrySetItemToPastureBox : GameAction
{
    public int pastureId;
    public int itemId;
    public int itemCount;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 0)
        {
            pastureId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
           itemId= int.Parse(parameters[1].value);
        }
        if (parameters.Count > 2)
        {
            itemCount= int.Parse(parameters[2].value);
        }
        if (source != 0)
        {
            pastureId = source;
        }
        if (target != 0)
        {
            itemId = target;
        }
        if (value != 0)
        {
            itemCount = value;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetPastureIndex : GameAction
{ 
    public int pastureId;
    public int index;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            pastureId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            index = int.Parse(parameters[1].value);
        }
         
        if (source != 0&&source!=int.MinValue)
        {
            pastureId = source;
        }
        if (target != 0 && target != int.MinValue)
        {
            index = target;
        } 
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct LinkPasturePackage : GameAction
{
    public int pastureInstance;
    public int foodPackage, waterPackage, productPackage;
    
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            pastureInstance = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            foodPackage = int.Parse(parameters[1].value);
        }
        if (parameters.Count > 2)
        {
            waterPackage = int.Parse(parameters[2].value);
        }
        if (parameters.Count > 3)
        {
            productPackage = int.Parse(parameters[3].value);
        }
        this.setResult = setResult;
        this.setValue = setValue;
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
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

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
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryDeletePasture : GameAction
{
    public int instanceId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

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
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshPasture : GameAction
{
    public int instanceId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

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
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}