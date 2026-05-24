using BehaviorDesigner.Runtime;
using System;
using UnityEngine;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(menuName ="Data/群体行为")]
public class MulitiBehaviorData : ScriptableObject, IGameData
{
    public string behaviorName;
    public int id;
    //public int2 characterCount;
    public ExternalBehaviorTree externalBehavior;
    public bool needRecoverSingleBehavior;
    public int endAction;
    public override string ToString()
    {
        return id.ToString();
    }
    public string GetKey()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        externalBehavior = AssetDatabase.LoadAssetAtPath<ExternalBehaviorTree>($"Assets/Resources/Behavior/MulitiBehavior/{behaviorName}.asset");
    }
#endif

}
