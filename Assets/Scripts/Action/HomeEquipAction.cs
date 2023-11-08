using Unity.Mathematics;

public struct CreatHomeEquip : GameAction
{
    public SetResult setResult { get; set; }
    public int characterId;
    public int equipDataId;
}
public struct RemoveHomeEquip : GameAction
{
    public SetResult setResult { get; set; }
    public int characterId;
    public int instanceId;
}
public struct ChangeHomeEquipCharacter : GameAction
{
    public SetResult setResult { get; set; }
    public int equipInstanceId;
    public int newPlayer;
}
public struct SetHomeEquipCoordinate : GameAction
{
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
    public SetResult setResult { get; set; }
}
public struct RefreshHomeEquip : GameAction
{
    public SetResult setResult { get; set; }
    public int equipInstanceId;
}
public struct RefreshCharacterHomeEquip : GameAction
{
    public SetResult setResult { get; set; }
    public int characterId;
}
public struct DisplayHomeEquipPanel : GameAction
{
    public SetResult setResult { get; set; }
    public int characterId;
}