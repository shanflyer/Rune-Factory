using System.Collections.Generic;
using UnityEngine;

public struct RefreshShortcut : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
}

public struct RemoveShortcutItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int index;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
    }
}

public struct SetShortcutItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int index;
    public Item Item;
}

public struct ChangeShortcutItemIndex : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int sourceIndex, targetIndex;
}

public struct RefreshItemValue : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int itemId;
    public float itemValue;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
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
    public List<ItemType> selectItemTypes;
    public PackageItemAction selectAction;
    public SetPanelReference setPanel;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count > 2)
        {
            packageId = int.Parse(parameters[0].value);
            selectActionName = parameters[1].value;
            selectActionId = int.Parse(parameters[2].value);
        }
        if (source != 0)
        {
            packageId = source;
        }
        if (value != -1)
        {
            selectActionId = value;
        }
        targetObj = target;
        GameActionManager.instance.QueueAction(this);
    }
}

public struct RemovePackageItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public int itemDataId;
    public int itemCount;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count >= 3)
        {
            packageId = int.Parse(parameters[0].value);
            itemDataId = int.Parse(parameters[1].value);
            itemCount = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}

public struct AddPackageItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public int itemDataId;
    public int itemCount;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count >= 3)
        {
            packageId = int.Parse(parameters[0].value);
            itemDataId = int.Parse(parameters[1].value);
            itemCount = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}

public struct CreatPackage : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageDataId;
    public int level;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if(parameters.Count >= 2)
        {
            packageDataId =int.Parse(parameters[0].value);
            level = int.Parse(parameters[1].value);
        }
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this,true);
    }
}

public struct RemovePackage : GameAction
{
    public int packageDataId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        GameActionManager.instance.QueueAction(this);
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

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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

        GameActionManager.instance.QueueAction(this);
    }
}

public struct RemoveRuntimePackage : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public Vector2Int key;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
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
        GameActionManager.instance.QueueAction(this);
    }
}

public struct ItemUseAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int packageId;
    public int itemId;
    public int itemCount;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null)
    {
        if (parameters.Count >= 3)
        {
            packageId = int.Parse(parameters[0].value);
            itemId = int.Parse(parameters[1].value);
            itemCount = int.Parse(parameters[2].value);
        }
        GameActionManager.instance.QueueAction(this);
    }
}