using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public struct CheckCharacterPackageFull : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId; 
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            characterId = int.Parse(parameters[0].value); 
        }
        if (source != 0)
            characterId = source; 

        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct CheckCharacterItemValue : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int itemId;
    public int itemValue;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 3)
        {
            characterId = int.Parse(parameters[0].value);
            itemId = int.Parse(parameters[1].value);
            itemValue = int.Parse(parameters[2].value);
        }
        if (source != 0)
            characterId = source;
        if (target != 0)
            itemId = target;
        if (value != -1)
            itemValue = value;

        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct CheckItemValue : GameAction
{
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }
    public int packageId;
    public int itemDataId; 
    public int itemValue; 
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
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
        if (value != -1)
            itemValue = value;

        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct GameCheckAction : GameAction
{
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CheckCharacterTemp : GameAction
{
    public int characterId;
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }
    public void Init(List<Parameter> parameters,int source=0,int target=0,int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}