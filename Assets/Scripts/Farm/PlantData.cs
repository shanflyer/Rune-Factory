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
                    stageObj = int.Parse(dataStr[4]),
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
    /// <summary>
    /// 状态名字
    /// </summary>
    public string stageName;
    /// <summary>
    /// 第几个状态
    /// </summary>
    public int stage;
    /// <summary>
    /// 生长周期
    /// </summary>
    public int growthDay;
    /// <summary>
    /// 产出？？
    /// </summary>
    public int productValue;
    /// <summary>
    /// 形象
    /// </summary>
    public int stageObj;
}