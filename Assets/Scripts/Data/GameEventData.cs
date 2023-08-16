using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameEventData : ScriptableObject,IGameData
{
    public int id;
    public string eventName;
    public string behaviorTreeName;
    public bool defaultAwake;

    public ExternalBehaviorTree behaviorTree;

    public override string ToString()
    {
        return id.ToString();
    }

#if UNITY_EDITOR
    public void SetReferenceData()
    {
        string path = $"{EditorDataPath.gameEventDataPath}{behaviorTreeName}{".asset"}";
        behaviorTree = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>(path);
    }

    public string GetKey()
    {
        return id.ToString();
    }
#endif

}
