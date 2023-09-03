using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/战斗地图")]
public class FightMapDataList : ScriptableObject, IGameData,IDataArray<FightMapData>
{
    [SerializeField]
    List<FightMapData> FightMapDatas;

    public List<FightMapData> DataList => FightMapDatas;
#if UNITY_EDITOR
    public void SetReferenceData()
    {
    }
#endif
    public string GetKey()
    {
        return name;
    }
}

[System.Serializable]
public struct FightMapData : IGameData
{
    public int id;
    public string mapName;
     
    public string iconName;
    public string backGroundName;
    public string fightMapObjName;
    public string exploreBGMName, fightBGMName;


    public float offsetY;
    public float cycleSize; 
    public Sprite Icon; 
    public Sprite Background; 
    public GameObject fightMapObj;
    public List<int> monsterDeploys;
    public List<GameAction> endMonsterAction;
    public AudioClip exploreBGM,fightBGM;
    public bool isOpen;
    public bool isZeroTeam;
    public int beforeActionId,afterActionId;
    public string battleNotice;
#if UNITY_EDITOR
    public void SetReferenceData()
    {
    }
#endif
    public string GetKey()
    {
        return id.ToString();
    }
}