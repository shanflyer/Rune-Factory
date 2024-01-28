using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/战斗地图")]
public class FightMapDataList : ScriptableObject, IGameData,IDataArray<FightMapData>
{
    [SerializeField]
    public FightMapData[] fightMapDatas;

    public FightMapData[] DataList => fightMapDatas;
#if UNITY_EDITOR
    public void SetReferenceData()
    {

        for(int i = 0; i < DataList.Length; i++)
        {
            var data = DataList[i];
            data.SetReferenceData();
            DataList[i] = data;
        } 
    }
#endif
    public string GetKey()
    {
        return "FightMapDataList";
    }
    public override string ToString()
    {
        return "FightMapDataList";
    }
}

[System.Serializable]
public struct FightMapData : IGameData
{ 
    public string mapName;
    public int id;

    public string fightMapObjName;
    public string exploreBGMName, fightBGMName;

    public Season season;
    public float offsetY;
    public float cycleSize;   
    public GameObject fightMapObj;
    public List<int> items;
    public List<int> monsterDeploys;
    public List<int> endMonsterEvents;
    public AudioClip exploreBGM,fightBGM;
    public bool isOpen;
    public bool isZeroTeam;
    public int beforeActionId,afterActionId;
    public int failureEventId;
    public int successEventId;
    public string battleNotice;
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        fightMapObj = Resources.Load<GameObject>($"Prefabs/FightMap/{fightMapObjName}");
    }
#endif
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
}