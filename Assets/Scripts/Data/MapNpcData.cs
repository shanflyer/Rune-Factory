using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[CreateAssetMenu(menuName ="Data/地图NPC")]
public class MapNpcData : ScriptableObject,IGameData
{
    public string npcName;
    public int id;
    public bool initialBeing;
    public int beingMap;
    public int2 beingCoordinate;
    public string behaviorName;

    public string GetKey()
    {
        return id.ToString();
    }
}

 