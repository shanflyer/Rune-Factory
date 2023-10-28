using System.Collections.Generic;
using UnityEngine;
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
    public void SetReferenceData()
    { 
    }
}
public struct GrowthStage
{
    public string stageName;
    public int stage;
    public int growthDay;
    public int productValue;
    public int objAnimationStage;
}