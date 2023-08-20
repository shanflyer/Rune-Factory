using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/世界数据")]
public class WorldMapData : ScriptableObject,IGameData
{
    public List<WorldMap> worldMaps = new List<WorldMap>();
    public List<MapLine> mapLines = new List<MapLine>();
#if UNITY_EDITOR
    public void SetReferenceData()
    {
    }
#endif
    public string GetKey()
    {
        return name;
    }
}

[System.Serializable]
public struct WorldMap
{
    public string map;
    public int id;
    public int3 coordinate;
}

[System.Serializable]
public struct MapLine
{
    public int map0, map1;
    public int2 cell0, cell1;
}