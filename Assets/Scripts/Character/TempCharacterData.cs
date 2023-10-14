using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BehaviorDesigner.Runtime;
using static BehaviorDesigner.Runtime.BehaviorManager;
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
    public IntBehaviorDictionary levelBehavior;
#if UNITY_EDITOR
    [HideInInspector]
    public List<int> levelDatas;
    [HideInInspector]
    public List<string> levelBehaviors;
    public void SetReferenceData()
    {
        string path = $"{EditorDataPath.tempCharacterBehaviorPath}{defaultBehaviorName}{".asset"}";
        defaultBehavior = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>(path);


        levelBehavior = new IntBehaviorDictionary();
        if (levelDatas != null)
        {
            for (int i = 0; i < levelDatas.Count; i++)
            {
                if (levelBehaviors.Count > i)
                {
                    path = $"{EditorDataPath.tempCharacterBehaviorPath}{levelBehaviors[i]}{".asset"}";
                    var behaviorTree = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>(path);
                    levelBehavior.Add(levelDatas[i], behaviorTree);
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

