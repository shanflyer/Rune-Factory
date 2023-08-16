using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using OldName;

public delegate Vector2 GetMoveVector();
public delegate void SetMoveTarge(int2 targetCoordinate, Vector2 targetPos);
[System.Serializable]
public struct Property
{
    public int MaxHP, HP, AT, DF, EXP, rewardEXP, NeedEXP, Power, MaxPower, Crit, Dodge;
    public Property(int zero)
    {
        MaxHP = 0;
        HP = 0;
        AT = 0;
        DF = 0;
        EXP = 0;
        rewardEXP = 0;
        NeedEXP = 0;
        Power = 0;
        MaxPower = 0;
        Crit = 0;
        Dodge = 0;
    }

    public Property(Property _property)
    {
        MaxHP = _property.MaxHP;
        HP = _property.HP;
        AT = _property.AT;
        DF = _property.DF;
        EXP = _property.EXP;
        NeedEXP = _property.NeedEXP;
        rewardEXP = _property.rewardEXP;
        Power = _property.Power;
        MaxPower = _property.MaxPower;
        Crit = _property.Crit;
        Dodge = _property.Dodge;
    }
    public static Property operator +(Property property0, Property property1)
    {
        Property result = new Property
        {
            MaxHP = property0.MaxHP + property1.MaxHP,
            HP = property0.HP + property1.HP,
            AT = property0.AT + property1.AT,
            DF = property0.DF + property1.DF,
            EXP = property0.EXP + property1.EXP,
            NeedEXP = property0.NeedEXP + property1.NeedEXP,
            rewardEXP = property0.rewardEXP + property1.rewardEXP,
            Power = property0.Power + property1.Power,
            MaxPower = property0.MaxPower + property1.MaxPower,
            Crit = property0.Crit + property1.Crit,
            Dodge = property0.Dodge + property1.Dodge


        };
        if (result.HP > result.MaxHP)
        {
            result.HP = result.MaxHP;
        }
        if (result.Power > result.MaxPower)
        {
            result.Power = result.MaxPower;
        }
        return result;
    }
    public static Property operator -(Property property0, Property property1)
    {
        Property result = new Property
        {
            MaxHP = property0.MaxHP - property1.MaxHP,
            HP = property0.HP - property1.HP,
            AT = property0.AT - property1.AT,
            DF = property0.DF - property1.DF,
            EXP = property0.EXP - property1.EXP,
            NeedEXP = property0.NeedEXP - property1.NeedEXP,
            rewardEXP = property0.rewardEXP - property1.rewardEXP,
            Power = property0.Power - property1.Power,
            MaxPower = property0.MaxPower - property1.MaxPower,
            Crit = property0.Crit - property1.Crit,
            Dodge = property0.Dodge - property1.Dodge
        };
        return result;
    }
    public int AddExp(int value, int professionId, int _level)
    {
        int Level = _level;
        EXP += value;
        ProfessionData professionData =
            GameComponentData.gameData.charactorDataAction.professionDatas0.Find(p => p.id == professionId);
        while (EXP >= NeedEXP)
        {
            EXP -= NeedEXP;
            Level++;
            Property levelProperty = professionData.GetPropertyFromLevelup(Level);
            this += levelProperty;
        }

        return Level;
    }
}
[System.Serializable]
public enum Gender
{
    male = 0,
    female = 1
}
[System.Serializable]
public enum ValueType
{
    AT = 1,
    DF = 2,
    MaxHp = 3,
    Hp = 4,
    Exp = 5,
    Power = 6,
    MaxPower = 7,
    Crit = 8,
    Dodge = 9,
}
[System.Serializable]
public enum CompareType
{
    等于, 不等于, 大于, 不大于, 小于, 不小于
}
[System.Serializable]
public enum CharacterPropertyType
{
    自定义值=-1, 体力,生命,法力,攻击,防御,暴击,闪避,饱食,
    最大体力,最大生命,最大法力
}
public enum Direction
{
   Default=-1, UP=0,RIGHT=3,DOWN=2,LEFT=1
}
public enum RuntimeObjType
{
    MAPGROUND, MAPITEM, CHARACTER
}
public static class CharacterAnimatorParameter
{
    public static int Speed=Animator.StringToHash("Speed");
    public static int Direction = Animator.StringToHash("Direction");
}
public enum EntityType
{
   All=1, 地图道具=2, 角色=15,玩家=3,NPC=5
}
public class GameCommon 
{
    public const int SeasonDays = 30;



    public const float cellWidth = 0.48f, cellHigh = 0.48f;
    public const float cellSize = 0.24f;
    public const float oneDividCellWidth = 2.0833f, oneDividCellHigh = 2.0833f;
    public const float slantValue = 0.707f;


    public const int randomInnerGroupMax = 5;

    public const int worldMapSizeX = 80;
    public const int worldMapSizeY = 45;
    public const float worldMapTileSize = 0.16f;
    

    public const float hightMin = -0.5f;
    public const float hightMax = 1;

    public const float waterPerlinMin = 4;
    public const float waterPerlinMax =6;

    public const float hightPerlinMin = 8;
    public const float hightPerlinMax = 10;

    public const int worldGridTypeSeedMin = 200;
    public const int worldGridTypeSeedMax = 2000;

    public const int hightSeedMin = 100;
    public const int hightSeedMax = 400;

    public const int waterSeedMin = 1000;
    public const int waterSeedMax = 10000;

    public const float waterMin = 0.01f;
    public const float waterMax = 0.02f;

    public const float waterRandomMin = 0;
    public const float waterRandomMax = 1;

    /// <summary>
    /// 生成水面的噪声时对结果的重映射
    /// </summary>
    public const float waterLerpValueMin_Min = 0.5f;
    public const float waterLerpValueMin_Max = 1f;
    public const float waterLerpValueMax_Min = 1f;
    public const float waterLerpValueMax_Max = 2.0f;


    public const float ScreenHalfSizeX = 960;
    public const float ScreenHalfSizeY = 540;

    public const string characterTriggerRenferenceName = "Entity";
    public const string triggerRenferenceName = "Reference";


    public static string AddString(string s0,string s1)
    { 
        var span = s1.AsSpan(); 
        var builder = new StringBuilder(s0);
        builder.Append(span); 
        return builder.ToString();
    }

    public static void SetEnable(GameObject gameObject,bool enable,bool compontEnable)
    {
        gameObject.transform.localScale = enable?Vector3.one:Vector3.zero;
    }
    public static int NextRandom(int numSeeds, int length)
    {
        // Create a byte array to hold the random value.    
        byte[] randomNumber = new byte[length];
        // Create a new instance of the RNGCryptoServiceProvider.    
        System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
        // Fill the array with a random value.    
        rng.GetBytes(randomNumber);
        // Convert the byte to an uint value to make the modulus operation easier.    
        uint randomResult = 0x0;
        for (int i = 0; i < length; i++)
        {
            randomResult |= ((uint)randomNumber[i] << ((length - 1 - i) * 8));
        }
        return (int)(randomResult % numSeeds) + 1;
    }
    public static int CreateRandSeed()
    {
        var iSeed = 0;
        var guid = Guid.NewGuid();
        iSeed = guid.GetHashCode();
        return iSeed;
    }
    public static Direction GetDirect(int2 start, int2 target)
    {
        int2 offset = start - target;

        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            if (offset.x < 0)
            {
                return Direction.RIGHT;
            }
            else
            {
                return Direction.LEFT;
            }
        }
        else
        {
            if (offset.y < 0)
            {
                return Direction.UP;
            }
            else
            {
                return Direction.DOWN;
            }
        }

        return Direction.Default;
    }
    public static Direction GetDirect(Vector2Int start,Vector2Int target)
    {
        Vector2Int offset = start - target;

        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            if (offset.x < 0)
            {
                return Direction.RIGHT;
            }
            else
            {
                return Direction.LEFT;
            }
        }
        else
        {
            if (offset.y < 0)
            {
                return Direction.UP;
            }
            else
            {
                return Direction.DOWN;
            }
        }

        return Direction.Default;
    }
    public static Direction GetCharacterDirect(int2 start, int2 target, Direction oldDirection = Direction.Default)
    {
        int2 offset = start - target;

        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            if (offset.x < 0)
            {
                return Direction.RIGHT;
            }
            else
            {
                return Direction.LEFT;
            }
        }
        if (Mathf.Abs(offset.x) < Mathf.Abs(offset.y))
        {
            if (offset.y < 0)
            {
                return Direction.UP;
            }
            else
            {
                return Direction.DOWN;
            }
        }
        if (offset.x!=0&& offset.y != 0)
        {
            if (offset.x < 0)
            {
                if (offset.y < 0)
                {
                    if (oldDirection == Direction.RIGHT || oldDirection == Direction.UP)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.UP;
                    }
                }
                else
                {
                    if (oldDirection == Direction.RIGHT || oldDirection == Direction.DOWN)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.DOWN;
                    }
                }
            }
            else
            {
                if (offset.y < 0)
                {
                    if (oldDirection == Direction.LEFT || oldDirection == Direction.UP)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.UP;
                    }
                }
                else
                {
                    if (oldDirection == Direction.LEFT || oldDirection == Direction.DOWN)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.DOWN;
                    }
                }
            }
        }
        return Direction.Default;
    }
    public static Direction GetCharacterDirect(Vector2Int start, Vector2Int target,Direction oldDirection=Direction.Default)
    {
        Vector2Int offset = start - target;

        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            if (offset.x < 0)
            {
                return Direction.RIGHT;
            }
            else
            {
                return Direction.LEFT;
            }
        }
        if (Mathf.Abs(offset.x) < Mathf.Abs(offset.y))
        {
            if (offset.y < 0)
            {
                return Direction.UP;
            }
            else
            {
                return Direction.DOWN;
            }
        }
        if (offset != Vector2Int.zero)
        {
            if (offset.x < 0)
            {
                if (offset.y < 0)
                {
                    if (oldDirection == Direction.RIGHT || oldDirection == Direction.UP)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.UP;
                    }
                }
                else
                {
                    if (oldDirection == Direction.RIGHT || oldDirection == Direction.DOWN)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.DOWN;
                    }
                }
            }
            else
            {
                if (offset.y < 0)
                {
                    if (oldDirection == Direction.LEFT || oldDirection == Direction.UP)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.UP;
                    }
                }
                else
                {
                    if (oldDirection == Direction.LEFT || oldDirection == Direction.DOWN)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.DOWN;
                    }
                }
            }
        }
        return Direction.Default;
    }
   
    public static float2 WorldCoordinateToPos(float2 coordinate)
    {
        float2 pos = new float2(coordinate.x * worldMapTileSize + worldMapTileSize * 0.5f, coordinate.y * worldMapTileSize + worldMapTileSize * 0.5f);
        return pos;
    }
    public static Vector2 GetMapPos(int x, int y)
    {
        Vector2 pos = new Vector2(cellWidth * x + cellWidth * 0.5f, cellHigh * y + cellHigh * 0.5f);
        return pos;
    }
    public static Vector2 GetMapPos(int2 coordinate)
    {
        Vector2 pos = new Vector2(cellWidth * coordinate.x + cellWidth * 0.5f, cellHigh * coordinate.y + cellHigh * 0.5f);
        return pos;
    }
    public static Vector2 GetMapPos(Vector2Int coordinate)
    {
        Vector2 pos = new Vector2(cellWidth * coordinate.x + cellWidth * 0.5f, cellHigh * coordinate.y + cellHigh * 0.5f);
        return pos;
    }
    public static Vector2 GetMapPos(Vector2 coordinate)
    {
        Vector2 pos = new Vector2(cellWidth * coordinate.x + cellWidth * 0.5f, cellHigh * coordinate.y + cellHigh * 0.5f);
        return pos;
    }
    public static Vector2 GetZeroMapPos(Vector2Int coordinate)
    {
        Vector2 pos = new Vector2(cellWidth * coordinate.x, cellHigh * coordinate.y);
        return pos;
    }
    public static Vector2 GetZeroMapPos(int2 coordinate)
    {
        Vector2 pos = new Vector2(cellWidth * coordinate.x, cellHigh * coordinate.y);
        return pos;
    }
    public static Vector2Int GetMapCoordinate(Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x * oneDividCellWidth);
        int y = Mathf.FloorToInt(pos.y * oneDividCellHigh);

        return new Vector2Int(x, y);
    }

    public static int2 GetMapCoordinateInt(Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x * oneDividCellWidth);
        int y = Mathf.FloorToInt(pos.y * oneDividCellHigh);

        return new int2(x, y);
    }
}

public static class DefaultGameData
{
    public const int defaultDay = 1;
    public const int defaultHour = 8;
    
}
public static class EditorDataPath
{
    public const string itemIconPath = "Item/";


    public const string outDataPath = "Assets/Resources/Data/";
    public const string groundSourcePath = "Assets/Texture/Map/Ground/";
    public const string sourceChangeNameDataPath = "Assets/Editor/Data/SourceChangeName.json";

    public const string mapItemDataPath = "Assets/Resources/Data/MapItemData/";
    public const string mapItemSourcePath = "Assets/Texture/Map/Item/";
    public const string mapItemPrefabPath = "Assets/Resources/Prefab/MapItem/";
    public const string mapRoomDataPath = "Assets/Resources/Data/Room/";
    public const string mapGroundPath = "Assets/Resources/Prefab/Ground/";

    public const string mapItemStructDataPath = "Assets/Editor/Data/MapItemStruct.json";
    public const string mapItemAnimationPath = "Assets/Animation/MapItem/";
    public const string worldMapDataPath = "Assets/Resources/Data/WorldMapData.asset";

    public const string gameEventDataPath = "Assets/Resources/Behavior/"; 


}
public static class DataPath
{
    public static Dictionary<Type, string> dataPathDic = new Dictionary<Type, string>
    {
        { typeof(LangLanguageSwitch),"Data/LangLanguageSwitchData" }
    }; 

    public static string GetDataPath(Type type)
    {
        if(dataPathDic.TryGetValue(type, out string path))
        {
            return path;
        }
        return null;
    }

    public const string BGMPath = "Audio/BGM/";
    public const string BGSPath = "Audio/BGS/";
    public const string MEPath = "Audio/ME/";
    public const string SEPath = "Audio/SE/";

    public const string titlePath = "ScriptableObject/Sprites/Title";
    public const string filmDataPath = "FilmObj/";

    public static string gameSaveDataPath = Application.persistentDataPath; 

    public const string gameRandomDataPath = "Data/GameRandomDataEditor/GameRandomDataList";
    public const string mapNpcDataPath = "Data/Character/MapNpcDataList";
    public const string worldDataPath = "Data/WorldMapData";
    public const string roomDataPath = "Data/Room/";
    public const string mapItemDataPath = "Data/MapItemData/";
    public const string itemDataPath = "Data/ItemData/";
    public const string gameActionDataPath = "Data/GameActionData/GameActionDataList";
    public const string gameEventDataPath = "Data/GameEventData/";
    public const string gameRadomDataPath= "Data/GameRandomData/GameRandomDataList";

    public const string itemAnimationDataPath = "Data/ItemAnimationData/";

    public const string characterPrefabPath = "Prefab/Character";
    public const string characterDataPath = "Data/CharacterData";
    public const string characterDataPathPath = "DataPath/CharacterData";

    public const string UIPath = "Prefabs/UI/";
}