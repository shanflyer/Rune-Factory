using Unity.Mathematics;

public struct CreatHomeEquip : GameAction
{
    public int characterId;
    public int equipDataId;
}
public struct RemoveHomeEquip : GameAction
{
    public int characterId;
    public int instanceId;
}
public struct ChangeHomeEquipCharacter : GameAction
{
    public int equipInstanceId;
    public int newPlayer;
}
public struct SetHomeEquipCoordinate : GameAction
{
    public int characterId;
    public int equipInstanceId;
    public int mapInstanceId;
    public int2 coordinate;
    public SetResult setResult;
}
public struct TryLayInHomeEquip : GameAction
{
    public int equipInstanceId;
    public int characterId;
    public SetResult setResult;
}
public struct RefreshHomeEquip : GameAction
{
    public int equipInstanceId;
}
public struct RefreshCharacterHomeEquip : GameAction
{
    public int characterId;
}
public struct DisplayHomeEquipPanel : GameAction
{
    public int characterId;
}