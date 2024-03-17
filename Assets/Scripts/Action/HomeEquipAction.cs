using Unity.Mathematics;

public struct CreatHomeEquip : GameAction
{
    public SetValue setValue { get; set; } public SetResult setResult { get; set; }
    public int characterId;
    public int itemDataId;
    public int equipDataId;
}
public struct RemoveHomeEquip : GameAction
{
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }
    public int characterId;
    public int instanceId;
}
public struct ChangeHomeEquipCharacter : GameAction
{
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }
    public int equipInstanceId;
    public int newPlayer;
}
public struct SetHomeEquipCoordinate : GameAction
{
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }
    public int characterId;
    public int equipInstanceId;
    public int mapInstanceId;
    public int2 coordinate;  
}
public struct TryLayInHomeEquip : GameAction
{
    public int equipInstanceId;
    public int characterId;
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }
}
public struct RefreshHomeEquip : GameAction
{
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }
    public int equipInstanceId;
}
public struct RefreshCharacterHomeEquip : GameAction
{
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }
    public int characterId;
}
public struct DisplayHomeEquipPanel : GameAction
{
     public SetValue setValue { get; set; } public SetResult setResult { get; set; }
    public int characterId;
}