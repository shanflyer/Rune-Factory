using BehaviorDesigner.Runtime;
using Unity.Mathematics;
using System.Collections.Generic;
using System;

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

   
#if UNITY_EDITOR

    public void SetReferenceData()
    { 
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



 