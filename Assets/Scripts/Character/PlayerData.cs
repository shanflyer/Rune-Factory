using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UserGameSaveData
{ 
    public CharacterSaveData playerData;
    public OtherSaveData otherSaveData;
    public List<CharacterSaveData> characterSaveDatas;
    public List<PackageSaveData> packageSaveDatas;
    public List<ChapterSave> chapters;
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
    public int boxPackageId, icePackageId;
    public bool isMarriedFood, isAnMo;
}
public struct CharacterSaveData:IReferenceData
{
    public string name;
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
 