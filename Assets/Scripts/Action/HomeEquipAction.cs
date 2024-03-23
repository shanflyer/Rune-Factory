using System.Collections.Generic;
using Unity.Mathematics;

public struct UnSetHomeEquip : GameAction
{
    public int instanceId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}

public struct CreatHomeEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int itemDataId;
    public int equipDataId;
    public int instanceId;
}

public struct RemoveHomeEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int instanceId;
}

public struct ChangeHomeEquipCharacter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int equipInstanceId;
    public int newPlayer;
}

public struct SetHomeEquipCoordinate : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
    public int equipInstanceId;
    public int mapInstanceId;
    public int2 coordinate;
}

public struct TryLayInHomeEquip : GameAction
{
    public int equipInstanceId;
    public int characterId;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}

public struct RefreshHomeEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int equipInstanceId;
}

public struct RefreshCharacterHomeEquip : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
}

public struct DisplayHomeEquipPanel : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int characterId;
}

public struct CreatManufature : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public int manufatureId;
    public int instanceId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            manufatureId = int.Parse(parameters[0].value);
            instanceId = int.Parse(parameters[1].value);
        }

        GameActionManager.instance.QueueAction(this, immediately);
    }
}