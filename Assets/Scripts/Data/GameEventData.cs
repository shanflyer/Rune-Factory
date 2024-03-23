using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;

#if UNITY_EDITOR

using UnityEditor;

#endif

[CreateAssetMenu(menuName = "Datas/事件数据")]
public class GameEventData : ScriptableObject, IGameData
{
    public int id;
    public string eventName;
    public string behaviorTreeName;
    public bool defaultAwake;
    public bool bindEvent = true;
    public List<EventReferenceData> eventReferenceDatas = new List<EventReferenceData>();
    public ExternalBehaviorTree behaviorTree;

    public override string ToString()
    {
        return id.ToString();
    }

    public string GetName()
    {
        return eventName;
    }

#if UNITY_EDITOR

    public void SetReferenceData()
    {
        string path = $"{EditorDataPath.gameEventDataPath}{behaviorTreeName}{".asset"}";
        behaviorTree = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>(path);
    }

#endif

    public string GetKey()
    {
        return id.ToString();
    }
}