using System;
using System.Collections.Generic;
public struct OpenOrCloseInputMap : GameAction
{
    public bool open;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 1)
        {
            open = bool.Parse(parameters[0].value);
        }
        else
        {
            open = source == 1;
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SwitchInputMap : GameAction
{
    public bool UI;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 1)
        {
            UI = bool.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ClosePanelAction : GameAction
{
    public Type type;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 1)
        {
            type = Type.GetType(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct OpenPanelAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public Type type;
    public string dataId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 2)
        {
            type = Type.GetType(parameters[0].value);
            dataId = parameters[1].value;
        }
        else
        {
            if (parameters.Count >= 1)
            {
                type = Type.GetType(parameters[0].value);
                dataId = source.ToString();
            }
        }
        if(target!= 0&&target!=int.MinValue)
        {
            dataId = target.ToString();
        }


        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct HidePanel : GameAction
{
    public bool hide;
    public Type type;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 1)
        {
            hide = bool.Parse(parameters[0].value);
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct ShowMultiPackagePanel : GameAction
{
    public int packageId0, packageId1;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately=false)
    {
        if (parameters.Count >= 2)
        {
            packageId0 = int.Parse(parameters[0].value);
            packageId1 = int.Parse(parameters[1].value);
        }
        if (packageId0 == 0)
        {
            if(source == 0)
            {
                packageId0 = CharacterManager.instance.controllerCharacter.characterPackage;
            }
            else
            {
                packageId0 = source;
            }
        }
        if (packageId1 == 0)
        {
            if (target == 0)
            {
                packageId1 = CharacterManager.instance.controllerCharacter.characterPackage;
            }
            else
            {
                packageId1 = target;
            }
        }
        GameActionManager.instance.QueueAction(this, immediately);
    }
}