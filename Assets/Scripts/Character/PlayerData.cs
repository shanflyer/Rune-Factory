using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
public class UserGameSaveDataList : IReferenceData
{
    public UserGameSaveData nowSaveData;
    public List<UserGameSaveData> userGameSaveDatas=new List<UserGameSaveData>();
}
public class UserGameSaveData :IReferenceData
{ 
    public CharacterSaveData playerData;
    public OtherSaveData otherSaveData;
    public GameDateSaveData dateData;
    public List<CharacterSaveData> characterSaveDatas;
    public List<PackageSaveData> packageSaveDatas;
    public List<ChapterSave> chapters;
    public List<FishSaveData> fishSaveDatas;

    public List<MapSaveData> mapSaveDatas;
    public IntInt4Dictionary changeMapItems;
    public IntInt3Dictionary SetAnimationStateMapItems;
    public List<int2> removeMapItemOperates;
    public List<int2> addMapItemOperates;

    public List<int> RemoveMapItemCollider;
    public List<int> deleteMapLine = new List<int>();
   

    public string saveTime;
    public int index;

    private HashSet<int> RemoveMapItemColliderSet = new HashSet<int>();
    private HashSet<int2> removeMapItemOperatesSet = new HashSet<int2>();
    private HashSet<int2> addMapItemOperatesSet = new HashSet<int2>();
    public void Init()
    {
        RemoveMapItemColliderSet.Clear();
        for(int i = 0; i < RemoveMapItemCollider.Count; i++)
        {
            RemoveMapItemColliderSet.Add(RemoveMapItemCollider[i]);
        }
        removeMapItemOperatesSet.Clear();
        addMapItemOperatesSet.Clear();
        for(int i=0;i< removeMapItemOperates.Count; i++)
        {
            removeMapItemOperatesSet.Add(removeMapItemOperates[i]);
        }
        for (int i = 0; i < addMapItemOperates.Count; i++)
        {
            addMapItemOperatesSet.Add(addMapItemOperates[i]);
        }
    }
    public void SaveData()
    {
        RemoveMapItemCollider.Clear();
        foreach(var itemId in RemoveMapItemColliderSet)
        {
            RemoveMapItemCollider.Add(itemId);
        }
        removeMapItemOperates.Clear();
        addMapItemOperates.Clear();
        foreach(var id in removeMapItemOperatesSet)
        {
            removeMapItemOperatesSet.Add(id);
        }
        foreach(var id in addMapItemOperatesSet)
        {
            addMapItemOperates.Add(id);
        }
    }
    public void RemoveMapItemOperate(int2 itemOperate)
    {
        removeMapItemOperatesSet.Add(itemOperate);
        addMapItemOperatesSet.Remove(itemOperate);
    }
    public void AddMapItemOperate(int2 itemOperate)
    {
        addMapItemOperatesSet.Add(itemOperate);
        removeMapItemOperatesSet.Remove(itemOperate);
    }

    public void AddRemoveMapItemColliderData(int id)
    {
        RemoveMapItemColliderSet.Add(id);
    }
    public void AddReSetMapItemColliderData(int id)
    {
        RemoveMapItemColliderSet.Remove(id); 
    }


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
    public void AddAnimationStateMapItem(int3 data)
    {
        if (SetAnimationStateMapItems == null)
        {
            SetAnimationStateMapItems = new IntInt3Dictionary();
        } 
        SetAnimationStateMapItems[data.x] = data;
    }
    public void AddChangeMapItem(int4 value)
    {
        if (changeMapItems == null)
        {
            changeMapItems = new IntInt4Dictionary();
        }
        
        changeMapItems[value.x] = value; 
    }

}
public struct ChangeMapItemSaveData
{

}
public struct MapSaveData
{
    public int mapId;
    public List<MapItemSaveData> mapItemSaveDatas;
}

public struct MapItemSaveData
{
    public int editorId;
    public int instanceId;
    public int state;
    public int2 pos;
}


public struct FishSaveData
{
    public int dataId;
    public int length;
    public List<int> places;
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
 