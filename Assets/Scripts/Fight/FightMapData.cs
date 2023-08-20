using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightMapDataList : ScriptableObject, IGameData,IDataArray<FightMapData>
{
    [SerializeField]
    List<FightMapData> FightMapDatas;

    public List<FightMapData> DataList => FightMapDatas;

    public string GetKey()
    {
        return name;
    }
}
[System.Serializable]
public struct MonsterDeploy
{
    public int refreshId;
    public int beforActionId;
    public int afterActionId;
    public int victoryId;
    public int failedId;
}

[System.Serializable]
public struct FightMapData:IGameData
{
    public int id;
    public string mapName;
    public string IconName;
    public string fightMapName;
    public float offsetY;
    public float cycleSize;
    public Sprite Icon;
    public Sprite Background;
    public GameObject fightMapObj;
    public List<MonsterDeploy> monsterDeploys;
    public bool isOpen;
    public int actionId;
    public string battleNotice;
    public string GetKey()
    {
        return id.ToString();
    }
}