using System;
using System.Collections.Generic; 
using Unity.Mathematics;
using UnityEngine;

public class PastureDataList : ScriptableObject, IGameData,IDataArray<PastureData>
{
    [SerializeField]
    private PastureData[] pastureDatas;
    public PastureData[] DataList => pastureDatas;

    public string GetKey()
    {
        return "PastureDataList";
    }
    public override string ToString()
    {
        return "PastureDataList";
    }
#if UNITY_EDITOR
    public PastureLevelData[] pastureLevelDatas;
    public void SetReferenceData()
    {
        List<PastureData> pastureDataList = new List<PastureData>();
        int pastureId = 0;
        for(int i = 0; i < pastureLevelDatas.Length; i++)
        {
            var pastureLevelData = pastureLevelDatas[i];

            PastureData pastureData;
            int pastureIndex=pastureDataList.FindIndex(p=>p.id == pastureId);
            if (pastureIndex <0)
            {
                pastureData = new PastureData
                {
                    id = pastureId,
                    pastureName = pastureLevelData.pastureName,
                    levelDatas = new List<PastureLevelData>
                    {
                        pastureLevelData
                    }
                };
                pastureDataList.Add(pastureData);
            }
            else
            {
                pastureData = pastureDataList[pastureIndex];
                pastureData.levelDatas.Add(pastureLevelData);
                pastureDataList[pastureIndex] = pastureData;
            }
        }
        pastureDatas = pastureDataList.ToArray();
    }
#endif 
}
public struct PastureLevelData
{
#if UNITY_EDITOR
    public int id;
    public string pastureName;
    public int productPackage;
    public int foodPackage;
    public int waterPackage;
#endif 
    public int level;
    public int linkItem;
    public int creatMoney;
    public List<int2> creatItems;
    public int animalCase;
}
public struct PastureData : IReferenceData, IGameData
{
    public int id;
    public string pastureName;
    public int productPackage;
    public int foodPackage;
    public int waterPackage;
    public List<PastureLevelData> levelDatas;

    public string GetKey()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    { 
    }
}