using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct MapItem
{
    public int id;
    public int instanceId;
    public int2 coordinate;
    public int animationKey;
}
[System.Serializable]
public struct MapCellData
{
    public Vector2Int coordinate;
    public bool isWalkable;
}

public class MapRoomData : ScriptableObject,IGameData
{
    public string roomName;
    public List<MapCellData> mapCells = new List<MapCellData>();
    public List<MapItem> mapItems = new List<MapItem>();
    public int2 startCoornate, endCoordinate;
    public GameObject mapObj;

    public bool CheckBoundary(int2 coordinate)
    {
        if (coordinate.x <= endCoordinate.x && coordinate.x >= startCoornate.x
            &&coordinate.y<= endCoordinate.y && coordinate.y >= startCoornate.y)
        {
            return true;
        }
        return false;
    }

    public string GetKey()
    {
        return roomName;
    }
}