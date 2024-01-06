using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class PlantData : ScriptableObject, IGameData
{
    public int id;
    public string plantName;
    public int seed;
    public int fruit;
    public int fruitCount;
    public int pickTimes;
    public int cycleStage;
    public int mapItem;

#if UNITY_EDITOR
    public string GrowthStageStr; 
#endif
    public List<GrowthStage> growthStages = new List<GrowthStage>();

    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        var strs = GrowthStageStr.Split('|');
        for(int i = 0; i < strs.Length; i++)
        {
            var dataStr = strs[i].Split(',');
            
            if (dataStr.Length >= 5)
            {
                GrowthStage growthStage = new GrowthStage
                {
                    stageName = dataStr[0],
                    stage = int.Parse(dataStr[1]),
                    growthDay = int.Parse(dataStr[2]),
                    productValue = int.Parse(dataStr[3]),
                    objAnimationStage = int.Parse(dataStr[4]),
                };
                growthStages.Add(growthStage);
            }
        }
    }
#endif

}
[Serializable]
public struct GrowthStage
{
    public string stageName;
    public int stage;
    public int growthDay;
    public int productValue;
    public int objAnimationStage;
}