using System.Collections.Generic;
using Unity.Mathematics;

public struct UnSetHomeEquip : GameAction
{
    public int instanceId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CreatHomeEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int equipDataId;
    public int instanceId;
    public int saveId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        if (source != int.MinValue) characterId = source;

        if (target != 0 && target != int.MinValue) equipDataId = target;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RemoveHomeEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int instanceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ChangeHomeEquipCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int equipInstanceId;
    public int newPlayer;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetHomeEquipCoordinate : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int equipInstanceId;
    public int mapInstanceId;
    public int2 coordinate;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryLayInHomeEquip : GameAction
{
    public int equipInstanceId;
    public int characterId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshHomeEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int equipInstanceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshCharacterHomeEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DisplayHomeEquipPanel : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetManufature : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public Manufature manufature;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct CreatManufature : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int manufatureId;
    public int instanceId;
    public int saveId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            manufatureId = int.Parse(parameters[0].value);
            instanceId = int.Parse(parameters[1].value);
        }
        if (source != 0 && source != int.MinValue)
        {
            instanceId = source;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct ClearManufature : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int manufatureId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            manufatureId = int.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct OpenFormula : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int formulaId;
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            formulaId = int.Parse(parameters[0].value);
        }
        if (source > 0)
            formulaId = source;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RefreshManufature : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public Manufature manufature;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

}
public struct UpdataManufature : GameAction
{
    public int manufatureId;
    public int waitTime;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
