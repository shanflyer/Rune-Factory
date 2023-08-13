using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UserGameSaveData
{
    public CharacterSaveData playerData; 
    public List<CharacterSaveData> characterSaveDatas;
    public List<PackageSaveData> packageSaveDatas;
}
public struct CharacterSaveData
{
    public string name;
    public Gender gender;
    public BrithDay brithDay;
    public int packageId;
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
 