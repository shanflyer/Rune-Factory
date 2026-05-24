using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_EDITOR
#endif

public class AnimalData : ScriptableObject, IGameData
{
    public string animalName;
    public int id;
    public int goodId;
    public int linkCharacter;
    public int age;
    public List<int> foods=new List<int>();
    public int produceCycle;
    public int productCount;
    public int productCD;
    public int getFoodEmote;
    public int hungerEmote;
    public int productEmote;
    public int2 talkId;
    public List<int> dailyTasks = new List<int>();
    public string behavior;

    public ExternalBehaviorTree externalBehavior => ExtensionsResources.LoadResource<ExternalBehaviorTree>(GameCommon.BlendString(
        DataPath.npcBehaviorPath,
        behavior));

#if UNITY_EDITOR
    public string GrowthStageStr;
#endif
    public List<GrowthStage> growthStages = new List<GrowthStage>();
    public List<int> functionIds;
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
        for (int i = 0; i < strs.Length; i++)
        {
            var dataStr = strs[i].Split(',');

            if (dataStr.Length >= 4)
            {
                GrowthStage growthStage = new GrowthStage
                {
                    stageName = dataStr[0],
                    stage = i,
                    growthHour = int.Parse(dataStr[1]),
                    productValue = int.Parse(dataStr[2]),
                    stageObj = int.Parse(dataStr[3]),
                };
                growthStages.Add(growthStage);
            }
        }

    }
#endif
}
