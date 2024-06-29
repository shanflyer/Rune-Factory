using System;
using System.Collections.Generic; 
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Purchasing;

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
    [HideInInspector]
    public PastureLevelData[] pastureLevelDatas;
    public void SetReferenceData()
    {
        List<PastureData> pastureDataList = new List<PastureData>();
        int pastureId = 0;
        for(int i = 0; i < pastureLevelDatas.Length; i++)
        {
            var pastureLevelData = pastureLevelDatas[i];
            pastureId = pastureLevelData.id;
            PastureData pastureData;
            int pastureIndex=pastureDataList.FindIndex(p=>p.id == pastureId);
            if (pastureIndex <0)
            {
                pastureData = new PastureData
                {
                    id = pastureId,
                    productPackage=pastureLevelData.productPackage,
                    foodPackage=pastureLevelData.foodPackage,
                    waterPackage=pastureLevelData.waterPackage,
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
[Serializable]
public struct PastureLevelData
{
#if UNITY_EDITOR
    [HideInInspector]
    public int id;
    [HideInInspector]
    public string pastureName;
    [HideInInspector]
    public int productPackage;
    [HideInInspector]
    public int foodPackage;
    [HideInInspector]
    public int waterPackage;
#endif 
    public int level;
    public int2 animationKey;
    public int creatMoney;
    public List<int2> creatItems;
    public int animalCase;
    public int successTalk;
}
[Serializable]
public class PastureData : IReferenceData, IGameData
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