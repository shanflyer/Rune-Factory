using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[CreateAssetMenu(menuName ="Data/地图NPC")]
public class MapNpcData : ScriptableObject,IGameData
{
    public string npcName;
    public int id;
    public int dataId;
    public bool initialBegin;
    public int beginMap;
    public int2 beginCoordinate;
    public string behaviorName;
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

 