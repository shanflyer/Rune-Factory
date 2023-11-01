using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct UserGameSaveDataList : IReferenceData
{
    public UserGameSaveData nowSaveData;
    public List<UserGameSaveData> userGameSaveDatas;
}
public struct UserGameSaveData:IReferenceData
{ 
    public CharacterSaveData playerData;
    public OtherSaveData otherSaveData;
    public GameDateSaveData dateData;
    public List<CharacterSaveData> characterSaveDatas;
    public List<PackageSaveData> packageSaveDatas;
    public List<ChapterSave> chapters;
    public string saveTime;
    public int index;
    public static UserGameSaveData CreatSaveData(int index)
    {
        OtherSaveData otherSaveData = new OtherSaveData
        {
            playerPackages = new List<int>()
        };

        UserGameSaveData userGameSaveData = new UserGameSaveData
        {
            otherSaveData = otherSaveData,
            characterSaveDatas = new List<CharacterSaveData>(),
            packageSaveDatas = new List<PackageSaveData>(),
            chapters = new List<ChapterSave>(),
            saveTime="1989",
            index = index
        };

        return userGameSaveData;
    }
}
 
public struct GameDateSaveData
{
    public int year;
    public Season season;
    public int day;
}
 
public struct ChapterSave
{
    public int mapId;
    public int completeValue;
    public List<int> findItems;
    public bool open;
}
public struct OtherSaveData
{
    public int gold, diamond;
    public List<int> playerPackages;
    public bool isMarriedFood, isAnMo;
}
public struct CharacterSaveData:IReferenceData
{
    public string name;
    public int characterId;
    public int level;
    public int exp;
    public Gender gender;
    public BrithDay brithDay;
    public int packageId;
    public int weapon, clothes;
    public bool isMarried;
}

public struct PackageSaveData
{
    public int caseCount;
    public int id;
    public int dataId;
    public int level;
    public PackageType packageType;
    public string packageName;
    public bool itemPackage;
    public List<Item> items;
}
public struct BrithDay
{
    public int year;
    public Season season;
    public int day;
}
 