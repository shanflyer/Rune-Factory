using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/世界数据")]
public class WorldMapData : ScriptableObject, IGameData
{
    public int defaultMap;
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
    public int eventId;
}

[System.Serializable]
public struct MapLine
{
    public int instanceId;
    public bool zeroInit;
    public int map0, map1;
    public LinkMapCell cells0, cells1;
    public int beforeActionId, afterActionId;

    public int2 center0
    {
        get
        {
            return cells1.targetCell.xy;
        }
    }

    public int2 center1
    {
        get
        {
            return cells0.targetCell.xy;
        }
    }
}

[System.Serializable]
public struct LinkMapCell
{
    public List<Direction> directions;
    public List<int2> cells;
    public int3 targetCell;
    public int beforAction, afterAction, checkAction;

    public int2 center
    {
        get
        {
            int minX = 10000, minY = 10000, maxX = -10000, maxY = -10000;
            if (cells == null)
            {
                cells = new List<int2>();
                return int2.zero;
            }
            for (int i = 0; i < cells.Count; i++)
            {
                if (minX > cells[i].x)
                {
                    minX = cells[i].x;
                }
                if (minY > cells[i].y)
                {
                    minY = cells[i].y;
                }
                if (maxX < cells[i].x)
                {
                    maxX = cells[i].x;
                }
                if (maxY < cells[i].y)
                {
                    maxY = cells[i].y;
                }
            }
            return new int2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);
        }
    }
}