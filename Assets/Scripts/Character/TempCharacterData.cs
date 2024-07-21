using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using Unity.Mathematics;

#if UNITY_EDITOR

using UnityEditor;

#endif

public class TempCharacterData : ScriptableObject, IGameData
{
    public int id;
    public string tempName;
    public int linkCharacterId;
    public string defaultBehaviorName;
    public ExternalBehaviorTree defaultBehavior;
    public IntBehaviorDictionarys levelMapBehaviors;
    public int defaultEmote;
    public IntIntDictionary mapEmote;
    public IntIntDictionary areaEmote;
    public int defaultTalk;
    public IntIntDictionary mapTalk;
    public IntIntDictionary areaTalk;
#if UNITY_EDITOR
    private List<int2> levelDatas;
    private List<string> levelBehaviors;

    public void SetReferenceData()
    {
        string path = $"{EditorDataPath.tempCharacterBehaviorPath}{defaultBehaviorName}{".asset"}";
        defaultBehavior = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>(path);

        levelMapBehaviors = new IntBehaviorDictionarys();
        if (levelDatas != null)
        {
            for (int i = 0; i < levelDatas.Count; i++)
            {
                if (levelBehaviors.Count > i)
                {
                    path = $"{EditorDataPath.tempCharacterBehaviorPath}{levelBehaviors[i]}{".asset"}";
                    var behaviorTree = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>(path);

                    int level = levelDatas[i].x;
                    int map = levelDatas[i].y;
                    if(!levelMapBehaviors.TryGetValue(level,out var behaviorDic))
                    {
                        behaviorDic = new IntBehaviorDictionary();
                        levelMapBehaviors.Add(level, behaviorDic);
                    }
                    behaviorDic[map] = behaviorTree; 
                }
            }
        }
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