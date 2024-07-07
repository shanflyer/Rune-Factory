 
using System;
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
    public int blindHomeEquipment;

    public List<MapItemEventReferenceData> eventReferenceDatas;
}

[System.Serializable]
public struct MapCellData
{
    public int2 coordinate;
    public bool isWalkable;
}

public enum FlowCameraType
{
    Default, FlowX, FlowY
}

public enum BehaviorAreaType
{
    创建, 聚集, 消失
}

[Serializable]
public class NpcBehaviorArea
{
    public int Name;
    public int2 pos;
    public List<int2> cells = new List<int2>();
    public List<int> grids = new List<int>();
    public BehaviorAreaType behaviorAreaType;

    public void CellToGrid()
    {
        HashSet<int2> allCellPoints = new HashSet<int2>();
        grids.Clear();

        for (int i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            allCellPoints.Add(cell);
        }


        while (allCellPoints.Count > 0)
        {
            int2 startPoint = new int2(int.MinValue, int.MinValue);
            using (var e = allCellPoints.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    startPoint = e.Current;
                }
            }
            if (startPoint.x != int.MinValue)
            {
                int left = startPoint.x;
                int right = startPoint.x;
                int bottom = startPoint.y;
                int top = startPoint.y;

                bool match = true;
                while (match)
                {
                    left--;
                    int2 leftPoint = new int2(left, startPoint.y);
                    if (!allCellPoints.Contains(leftPoint))
                    {
                        left++;
                        match = false;
                        break;
                    }
                }

                match = true;
                while (match)
                {
                    right++;
                    int2 rightPoint = new int2(right, startPoint.y);
                    if (!allCellPoints.Contains(rightPoint))
                    {
                        right--;
                        match = false;
                        break;
                    }
                }

                match = true;
                while (match)
                {
                    bottom--;
                    for (int x = left; x <= right; x++)
                    {
                        int2 point = new int2(x, bottom);
                        if (!allCellPoints.Contains(point))
                        {
                            match = false;
                            bottom++;
                            break;
                        }
                    }
                }

                match = true;
                while (match)
                {
                    top++;
                    for (int x = left; x <= right; x++)
                    {
                        int2 point = new int2(x, top);
                        if (!allCellPoints.Contains(point))
                        {
                            match = false;
                            top--;
                            break;
                        }
                    }
                }

                for (int x = left; x <= right; x++)
                {
                    for (int y = bottom; y <= top; y++)
                    {
                        int2 point = new int2(x, y);
                        allCellPoints.Remove(point);
                    }
                }
                grids.Add(left);
                grids.Add(bottom);
                grids.Add(right);
                grids.Add(top);
            }
        }
        allCellPoints.Clear();
        cells.Clear();

    }
}

public class MapRoomData : ScriptableObject, IGameData
{
    public string roomName;
    public List<MapCellData> mapCells = new List<MapCellData>();

    public List<int> barrierGrids=new List<int>();

    public List<MapItem> mapItems = new List<MapItem>();
    public int2 startCoordinate, endCoordinate;
    public GameObject mapObj;
    public string dayEnvironmentDataName, duskEnvironmentDataName, dawnEnvironmentDataName, nightEnvironmentDataName;
    public bool displaySky = true;
    public bool displaySunlight = false;
    public bool fixedCamera;
    public FlowCameraType flowCameraType;
    public Vector3 fixedCameraPos;
    public int skyBackGroundId;
    public int creatTempCharacterId;

    public List<NpcBehaviorArea> npcBehaviorAreas = new List<NpcBehaviorArea>();

    public bool CheckBoundary(int2 coordinate)
    {
        if (coordinate.x <= endCoordinate.x && coordinate.x >= startCoordinate.x
            && coordinate.y <= endCoordinate.y && coordinate.y >= startCoordinate.y)
        {
            return true;
        }
        return false;
    }
    public void CellToGrid()
    {
        HashSet<int2> allCellPoints = new HashSet<int2>();
        barrierGrids.Clear();

        for (int i = 0; i < mapCells.Count; i++)
        {
            var cell = mapCells[i];
            if (!cell.isWalkable)
            {
                allCellPoints.Add(cell.coordinate);
            } 
        }


        while (allCellPoints.Count > 0)
        {
            int2 startPoint = new int2(int.MinValue, int.MinValue);
            using (var e = allCellPoints.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    startPoint = e.Current;
                }
            }
            if (startPoint.x != int.MinValue)
            {
                int left = startPoint.x;
                int right = startPoint.x;
                int bottom = startPoint.y;
                int top = startPoint.y;

                bool match = true;
                while (match)
                {
                    left--;
                    int2 leftPoint = new int2(left, startPoint.y);
                    if (!allCellPoints.Contains(leftPoint))
                    {
                        left++;
                        match = false;
                        break;
                    }
                }

                match = true;
                while (match)
                {
                    right++;
                    int2 rightPoint = new int2(right, startPoint.y);
                    if (!allCellPoints.Contains(rightPoint))
                    {
                        right--;
                        match = false;
                        break;
                    }
                }

                match = true;
                while (match)
                {
                    bottom--;
                    for (int x = left; x <= right; x++)
                    {
                        int2 point = new int2(x, bottom);
                        if (!allCellPoints.Contains(point))
                        {
                            match = false;
                            bottom++;
                            break;
                        }
                    }
                }

                match = true;
                while (match)
                {
                    top++;
                    for (int x = left; x <= right; x++)
                    {
                        int2 point = new int2(x, top);
                        if (!allCellPoints.Contains(point))
                        {
                            match = false;
                            top--;
                            break;
                        }
                    }
                }

                for (int x = left; x <= right; x++)
                {
                    for (int y = bottom; y <= top; y++)
                    {
                        int2 point = new int2(x, y);
                        allCellPoints.Remove(point);
                    }
                }
                barrierGrids.Add(left);
                barrierGrids.Add(bottom);
                barrierGrids.Add(right);
                barrierGrids.Add(top); 
            }
        }
        allCellPoints.Clear();
        mapCells.Clear();

    }
#if UNITY_EDITOR

    public void SetReferenceData()
    {
        if (mapCells.Count > 0)
        {
            CellToGrid();
        }
        foreach(var npcBehaviorArea in npcBehaviorAreas)
        {
            if (npcBehaviorArea.cells.Count > 0)
            {
                npcBehaviorArea.CellToGrid();
            }
        }
        
    }

#endif

    public string GetKey()
    {
        return roomName;
    }
}