using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public class AnimalData : ScriptableObject, IGameData
{ 
    public string animalName;
    public int id;
    public int goodId;
    public int linkCharacter;
    public int age;
    public List<int> foods=new List<int>();
    public int produceCycle;
    public int product;
    public int productCount;
    public int cycleStage;
    public string behavior;
    public ExternalBehaviorTree externalBehavior;

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
        for (int i = 0; i < strs.Length; i++)
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

        externalBehavior= AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>($"{EditorDataPath.npcBehaviorPath}{behavior}.asset");
    }
#endif
}