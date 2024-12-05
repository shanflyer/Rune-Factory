using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct AddItemValue : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId; 
    public int selectItem;
    public int value;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 2)
        {
            characterId = int.Parse(parameters[0].value);
            selectItem = int.Parse(parameters[1].value);
            this.value = int.Parse(parameters[2].value);
        }
        
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetItemValue : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int selectItem;
    public int value;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {

        if (parameters.Count > 2)
        {
            characterId = int.Parse(parameters[0].value);
            selectItem = int.Parse(parameters[1].value);
            this.value = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetPackageSelectItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public int selectItem;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RefreshShortcut : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SortShortcutItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int itemId;
    public int index;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RemoveShortcutItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int index;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetShortcutItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId; 
    public Item Item;
}

 

public struct RefreshItemValue : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int itemId;
    public float itemValue;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SortPackageItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public int itemId;
    public int index;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 2)
        {
            packageId = int.Parse(parameters[0].value);
            itemId = int.Parse(parameters[1].value);
            index = int.Parse(parameters[2].value);
        }
       
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct OpenPackage : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public string selectActionName;
    public int selectActionId;
    public int targetObj;
    public bool canSetShortcut;
    public bool eventAction;
    public ItemMatchData itemMatchData;
    public SelectAction<Item> selectAction;
    public SetPanelReference setPanel;
    public bool isMiniShow;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count > 2)
        {
            packageId = int.Parse(parameters[0].value);
            selectActionName = parameters[1].value;
            selectActionId = int.Parse(parameters[2].value);
            if (parameters.Count > 3)
            {
                canSetShortcut = int.Parse(parameters[3].value) != 0;
            }
            if (parameters.Count > 4)
            {
                eventAction = int.Parse(parameters[4].value) != 0;
            }
        }
        else {
            packageId = source;
            selectActionId = value;
            canSetShortcut = target != 0;
        }
        
        targetObj = target;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RemovePlayerPackageItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int itemDataId;
    public int itemCount;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 3)
        {
            characterId = int.Parse(parameters[0].value);
            itemDataId = int.Parse(parameters[1].value);
            itemCount = int.Parse(parameters[2].value);
        }
        if (source != 0)
            characterId = source;
        if (target != 0)
            itemDataId = target;
        if (value > 0)
            itemCount = value;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RemovePackageItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public int itemDataId;
    public int itemCount;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 3)
        {
            packageId = int.Parse(parameters[0].value);
            itemDataId = int.Parse(parameters[1].value);
            itemCount = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RemovePackageItemInstance : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public int itemInstanceId; 

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 3)
        {
            packageId = int.Parse(parameters[0].value);
            itemInstanceId = int.Parse(parameters[1].value); 
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct ChangePackageInnstance : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int oldInstanceId;
    public int newInstanceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 2)
        {
            oldInstanceId = int.Parse(parameters[0].value);
            newInstanceId = int.Parse(parameters[1].value); 
        }
        if (source != 0 && source != int.MinValue)
        {
            oldInstanceId = source;
        }
        if (target != 0 && target != int.MinValue)
        {
            newInstanceId = target;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }

}
public struct AddPackageItemList : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public List<int2> items; 
}
public struct AddPackageItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public int itemDataId;
    public int itemCount;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 3)
        {
            packageId = int.Parse(parameters[0].value);
            itemDataId = int.Parse(parameters[1].value);
            itemCount = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CreatPackage : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageDataId;
    public int level;
    public int instanceId;
    public bool playerPackage;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters != null)
        {
            if (parameters.Count >= 2)
            {
                packageDataId = int.Parse(parameters[0].value);
                level = int.Parse(parameters[1].value);
            }
            if (parameters.Count >= 3)
            {
                instanceId = int.Parse(parameters[2].value);
            }
            if (source != 0 && source != int.MinValue)
                instanceId = source;
            if (target != 0 && target != int.MinValue)
                packageDataId = target;
            if (value != 0 && value != int.MinValue)
                level = value;
        }
        else
        {
            instanceId = source;
            packageDataId = target;

        }
        
        this.setValue = setValue;
        this.setResult = setResult;
        GameActionManager.instance.QueueAction(this,true);
    }
}

public struct RemovePackage : GameAction
{
    public int packageDataId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CreatRuntimePackage : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public string name;
    public Vector2Int key;
    public int instanceId;
    public int caseCount;
    public List<Item> Items;
    public bool itemPackage;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        /*
        if (parameters.Count >= 6)
        {
            name = parameters[0].value;
            var parameter = parameters[1];
            if (parameter.parameters.Count >= 2)
            {
                key.x = int.Parse(parameter.parameters[0].value);
                key.y = int.Parse(parameter.parameters[1].value);
            }

            instanceId = int.Parse(parameters[2].value);
            caseCount = int.Parse(parameters[3].value);

            Items = new List<Item>();
            var itemParameter = parameters[4];
            if (itemParameter.parameters.Count > 0)
            {
                for(int i=0;i< itemParameter.parameters.Count; i++)
                {
                    var _Parameter = itemParameter.parameters[i];
                    if (_Parameter.parameters.Count >= 3)
                    {
                        Item item = new Item
                        {
                            instanceId = int.Parse(_Parameter.parameters[0].value),
                            dataId = int.Parse(_Parameter.parameters[1].value),
                            count= int.Parse(_Parameter.parameters[2].value)
                        };
                        Items.Add(item);
                    }
                }
            }

            itemPackage = bool.Parse(parameters[5].value);
        }

        */

        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RemoveRuntimePackage : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public Vector2Int key;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 1)
        {
            var parameter = parameters[0];
            if (parameter.parameters.Count >= 2)
            {
                key.x = int.Parse(parameter.parameters[0].value);
                key.y = int.Parse(parameter.parameters[1].value);
            }
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ItemUseAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public int itemId;
    public int itemCount;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 3)
        {
            packageId = int.Parse(parameters[0].value);
            itemId = int.Parse(parameters[1].value);
            itemCount = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}