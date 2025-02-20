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
    public int openLevel;
    private string plantIcon;
    public List<int> goodSeason=new List<int>();
    public List<int> badSeason = new List<int>();
    public Sprite icon;

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
    static Dictionary<string, Sprite> plantIconDic;
    public void SetReferenceData()
    {
        if (plantIconDic == null)
        {
            plantIconDic = new Dictionary<string, Sprite>();
            var icons = AssetDatabase.LoadAllAssetsAtPath("Assets/Texture/Farm/Farm.png");
            for (int i = 0; i < icons.Length; i++)
            {
                var icon = icons[i];
                if (icon is Sprite sprite)
                {
                    plantIconDic[sprite.name] = sprite;
                }
            }

        }
        plantIconDic.TryGetValue(plantIcon, out icon);

        var strs = GrowthStageStr.Split('|');
        for(int i = 0; i < strs.Length; i++)
        {
            var dataStr = strs[i].Split(',');
            
            if (dataStr.Length >= 3)
            {
                GrowthStage growthStage = new GrowthStage
                {
                    stageName = dataStr[0],
                    growthHour = int.Parse(dataStr[1]),
                    productValue = int.Parse(dataStr[2]),
                    stageObj =i,
                    stage = i
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
    public int growthHour;
    /// <summary>
    /// 产出？？
    /// </summary>
    public int productValue;
    /// <summary>
    /// 形象
    /// </summary>
    public int stageObj;
}