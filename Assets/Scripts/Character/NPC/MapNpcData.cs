using BehaviorDesigner.Runtime;
using Unity.Mathematics;
using System.Collections.Generic;

#if UNITY_EDITOR

using UnityEditor;

#endif

using UnityEngine;

[CreateAssetMenu(menuName = "Data/地图NPC")]
public class MapNpcData : ScriptableObject, IGameData
{
    public string npcName;
    public int id;
    public int dataId;
    public bool initialBegin;
    public int beginMap;
    public int2 beginCoordinate;

    public List<int> dailyTasks=new List<int>();
    public List<int> eventTasks = new List<int>();

    public string behaviorName;
    public ExternalBehaviorTree externalBehavior;
#if UNITY_EDITOR

    public void SetReferenceData()
    {
        externalBehavior = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>($"{EditorDataPath.npcBehaviorPath}{behaviorName}.asset");
    }

#endif

    public override string ToString()
    {
        return GetKey();
    }

    public string GetKey()
    {
        return id.ToString();
    }
}