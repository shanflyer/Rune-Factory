using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;

#if UNITY_EDITOR

using UnityEditor;

#endif
[Serializable]
public class IntListDictionary : SerializableDictionary<int, MyListInt>
{

}
[Serializable]
public class GridLineVector2Dictionary : SerializableDictionary<GridLine, Vector2>
{

}
[ExecuteAlways]
public class TestGrid : MonoBehaviour
{
    public Transform textParent;
    public TextMeshPro textMesh;
    public Tilemap tilemap;
    public List<int2> cells = new List<int2>();
    public List<int4> grids = new List<int4>();

    public List<int4> other = new List<int4>();
    public GridLineVector2Dictionary gridNeighborDic = new GridLineVector2Dictionary();

    public IntListDictionary gridNeighbors = new IntListDictionary();
    public MapRoomData roomData;
    public int2 key;

    // Use this for initialization
    private void Start()
    {
    }

    public void RemoveGridLine()
    {
        GridLine gridLine = new GridLine
        {
            grid0 = key.x,
            grid1 = key.y
        };
        gridNeighborDic.Remove(gridLine);
    }



    private bool IsInGrid(int2 cell, int4 grid)
    {
        if (cell.x >= grid.x && cell.x <= grid.z && cell.y >= grid.y && cell.y <= grid.w)
        {
            return true;
        }
        return false;
    }

    private bool CheckCrossGrid(int4 grid0, int4 grid1)
    {
        if (grid0.x > grid1.z || grid0.z < grid1.x || grid0.y > grid1.w || grid0.w < grid1.y)
        {
            return false;
        }
        return true;
    }

    public void InitOutCell()
    {
        HashSet<int2> allCells = new HashSet<int2>();
        List<int> removeGrids = new List<int>();

        for (int i = grids.Count - 1; i >= 0; i--)
        {
            for (int j = 0; j < other.Count; j++)
            {
                if (CheckCrossGrid(grids[i], other[j]))
                {
                    for (int x = grids[i].x; x <= grids[i].z; x++)
                    {
                        for (int y = grids[i].y; y <= grids[i].w; y++)
                        {
                            allCells.Add(new int2(x, y));
                        }
                    }

                    int left = grids[i].x;
                    int right = grids[i].z;
                    int bottom = grids[i].y;
                    int top = grids[i].w;

                    if (leftRange.TryGetValue(right, out var ints))
                    {
                        ints.intList.Remove(i);
                    }
                    if (rightRange.TryGetValue(left, out ints))
                    {
                        ints.intList.Remove(i);
                    }
                    if (topRange.TryGetValue(bottom, out ints))
                    {
                        ints.intList.Remove(i);
                    }
                    if (bottomRange.TryGetValue(top, out ints))
                    {
                        ints.intList.Remove(i);
                    }

                    removeGrids.Add(i);
                }
            }
        }

        for (int i = 0; i < removeGrids.Count; i++)
        {
            if (gridNeighbors.TryGetValue(removeGrids[i], out var ints))
            {
                for (int j = 0; j < ints.intList.Count; j++)
                {
                    gridNeighborDic.Remove(new GridLine { grid0 = removeGrids[i], grid1 = ints.intList[j] });
                }
            }
        }

        for (int j = 0; j < other.Count; j++)
        {
            for (int x = other[j].x; x <= other[j].z; x++)
            {
                for (int y = other[j].y; y <= other[j].w; y++)
                {
                    allCells.Remove(new int2(x, y));
                }
            }
        }

        while (allCells.Count > 0)
        {
            int2 startPoint = new int2(int.MinValue, int.MinValue);
            using (var e = allCells.GetEnumerator())
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
                    if (!allCells.Contains(leftPoint))
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
                    if (!allCells.Contains(rightPoint))
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
                        if (!allCells.Contains(point))
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
                        if (!allCells.Contains(point))
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
                        allCells.Remove(point);
                    }
                }
                grids.Add(new int4(left, bottom, right, top));

                int gridIndex = grids.Count - 1;
                if (leftRange.TryGetValue(right, out var leftList))
                {
                    leftList.intList.Add(gridIndex);
                }
                else
                {
                    leftList = new MyListInt { intList=new List<int>()};
                    leftList.intList.Add(gridIndex);
                    leftRange.Add(right, leftList);
                }
                if (rightRange.TryGetValue(left, out var rightList))
                {
                    rightList.intList.Add(gridIndex);
                }
                else
                {
                    rightList = new MyListInt { intList = new List<int>() };
                    rightList.intList.Add(gridIndex);
                    rightRange.Add(left, rightList);
                }
                if (bottomRange.TryGetValue(top, out var bottomList))
                {
                    bottomList.intList.Add(gridIndex);
                }
                else
                {
                    bottomList = new MyListInt { intList = new List<int>() };
                    bottomList.intList.Add(gridIndex);
                    bottomRange.Add(top, bottomList);
                }
                if (topRange.TryGetValue(bottom, out var topList))
                {
                    topList.intList.Add(gridIndex);
                }
                else
                {
                    topList = new MyListInt { intList = new List<int>() };
                    topList.intList.Add(gridIndex);
                    topRange.Add(bottom, topList);
                }

                int leftNeighbor = left - 1;
                int rightNeighbor = right + 1;
                int bottomNeighbor = bottom - 1;
                int topNeighbor = top + 1;
                if (leftRange.TryGetValue(leftNeighbor, out var gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.intList.Count; i++)
                    {
                        int checkTop = grids[gridIndexs.intList[i]].w;
                        int checkBottom = grids[gridIndexs.intList[i]].y;
                        if (checkTop >= bottom && checkBottom <= top)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs.intList[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkBottom, bottom), math.min(checkTop, top));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(leftNeighbor + 0.5f, center));
                            }
                        }
                    }
                }
                //gridIndexs.Clear();
                if (rightRange.TryGetValue(rightNeighbor, out gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.intList.Count; i++)
                    {
                        int checkTop = grids[gridIndexs.intList[i]].w;
                        int checkBottom = grids[gridIndexs.intList[i]].y;
                        if (checkTop >= bottom && checkBottom <= top)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs.intList[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkBottom, bottom), math.min(checkTop, top));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(rightNeighbor - 0.5f, center));
                            }
                        }
                    }
                }
                //gridIndexs.Clear();
                if (bottomRange.TryGetValue(bottomNeighbor, out gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.intList.Count; i++)
                    {
                        int checkRight = grids[gridIndexs.intList[i]].z;
                        int checkLeft = grids[gridIndexs.intList[i]].x;
                        if (checkRight >= left && checkLeft <= right)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs.intList[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkLeft, left), math.min(checkRight, right));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(center, bottomNeighbor + 0.5f));
                            }
                        }
                    }
                }
                //gridIndexs.Clear();
                if (topRange.TryGetValue(topNeighbor, out gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.intList.Count; i++)
                    {
                        int checkRight = grids[gridIndexs.intList[i]].z;
                        int checkLeft = grids[gridIndexs.intList[i]].x;
                        if (checkRight >= left && checkLeft <= right)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs.intList[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkLeft, left), math.min(checkRight, right));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(center, topNeighbor - 0.5f));
                            }

                            if (!gridNeighbors.TryGetValue(line.grid0, out var ints))
                            {
                                ints.intList = new List<int>();
                                gridNeighbors.Add(line.grid0, ints);
                            }
                            ints.intList.Add(line.grid1);

                            if (!gridNeighbors.TryGetValue(line.grid1, out var ints1))
                            {
                                ints1.intList = new List<int>();
                                gridNeighbors.Add(line.grid1, ints);
                            }
                            ints1.intList.Add(line.grid0);
                        }
                    }
                }
            }
        }
    }

    public IntListDictionary leftRange = new IntListDictionary();
    public IntListDictionary rightRange = new IntListDictionary();
    public IntListDictionary bottomRange = new IntListDictionary();
    public IntListDictionary topRange = new IntListDictionary();

    public void CellToGridInt4()
    {
        cells.Clear();
        for (int x = tilemap.cellBounds.xMin; x <= tilemap.cellBounds.xMax; x++)
        {
            for (int y = tilemap.cellBounds.yMin; y <= tilemap.cellBounds.yMax; y++)
            {
                var tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile != null && tile.name == "1")
                {
                    cells.Add(new int2(x, y));
                }
            }
        }
        float timeValue = Time.realtimeSinceStartup;
        grids.Clear();
        gridNeighborDic.Clear();
        leftRange.Clear(); rightRange.Clear(); bottomRange.Clear(); topRange.Clear();
        gridNeighbors.Clear();

        HashSet<int2> allCellPoints = new HashSet<int2>();

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
                grids.Add(new int4(left, bottom, right, top));

                int gridIndex = grids.Count - 1;
                if (leftRange.TryGetValue(right, out var leftList))
                {
                    leftList.intList.Add(gridIndex);
                }
                else
                {
                    leftList = new MyListInt { intList = new List<int>() };
                    leftList.intList.Add(gridIndex);
                    leftRange.Add(right, leftList);
                }
                if (rightRange.TryGetValue(left, out var rightList))
                {
                    rightList.intList.Add(gridIndex);
                }
                else
                {
                    rightList = new MyListInt { intList = new List<int>() };
                    rightList.intList.Add(gridIndex);
                    rightRange.Add(left, rightList);
                }
                if (bottomRange.TryGetValue(top, out var bottomList))
                {
                    bottomList.intList.Add(gridIndex);
                }
                else
                {
                    bottomList = new MyListInt { intList = new List<int>() };
                    bottomList.intList.Add(gridIndex);
                    bottomRange.Add(top, bottomList);
                }
                if (topRange.TryGetValue(bottom, out var topList))
                {
                    topList.intList.Add(gridIndex);
                }
                else
                {
                    topList = new MyListInt { intList = new List<int>() };
                    topList.intList.Add(gridIndex);
                    topRange.Add(bottom, topList);
                }

                int leftNeighbor = left - 1;
                int rightNeighbor = right + 1;
                int bottomNeighbor = bottom - 1;
                int topNeighbor = top + 1;
                if (leftRange.TryGetValue(leftNeighbor, out var gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.intList.Count; i++)
                    {
                        int checkTop = grids[gridIndexs.intList[i]].w;
                        int checkBottom = grids[gridIndexs.intList[i]].y;
                        if (checkTop >= bottom && checkBottom <= top)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs.intList[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkBottom, bottom), math.min(checkTop, top));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(leftNeighbor + 0.5f, center));
                            }

                            if (!gridNeighbors.TryGetValue(line.grid0, out var ints))
                            {
                                ints.intList = new List<int>();
                                gridNeighbors.Add(line.grid0, ints);
                            }
                            ints.intList.Add(line.grid1);

                            if (!gridNeighbors.TryGetValue(line.grid1, out var ints1))
                            {
                                ints1.intList = new List<int>();
                                gridNeighbors.Add(line.grid1, ints1);
                            }
                            ints1.intList.Add(line.grid0);
                        }
                    }
                }
                //gridIndexs.Clear();
                if (rightRange.TryGetValue(rightNeighbor, out gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.intList.Count; i++)
                    {
                        int checkTop = grids[gridIndexs.intList[i]].w;
                        int checkBottom = grids[gridIndexs.intList[i]].y;
                        if (checkTop >= bottom && checkBottom <= top)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs.intList[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkBottom, bottom), math.min(checkTop, top));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(rightNeighbor - 0.5f, center));
                            }

                            if (!gridNeighbors.TryGetValue(line.grid0, out var ints))
                            {
                                ints.intList = new List<int>();
                                gridNeighbors.Add(line.grid0, ints);
                            }
                            ints.intList.Add(line.grid1);

                            if (!gridNeighbors.TryGetValue(line.grid1, out var ints1))
                            {
                                ints1.intList = new List<int>();
                                gridNeighbors.Add(line.grid1, ints1);
                            }
                            ints1.intList.Add(line.grid0);
                        }
                    }
                }
                //gridIndexs.Clear();
                if (bottomRange.TryGetValue(bottomNeighbor, out gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.intList.Count; i++)
                    {
                        int checkRight = grids[gridIndexs.intList[i]].z;
                        int checkLeft = grids[gridIndexs.intList[i]].x;
                        if (checkRight >= left && checkLeft <= right)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs.intList[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkLeft, left), math.min(checkRight, right));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(center, bottomNeighbor + 0.5f));
                            }

                            if (!gridNeighbors.TryGetValue(line.grid0, out var ints))
                            {
                                ints .intList= new List<int>();
                                gridNeighbors.Add(line.grid0, ints);
                            }
                            ints.intList.Add(line.grid1);

                            if (!gridNeighbors.TryGetValue(line.grid1, out var ints1))
                            {
                                ints1.intList = new List<int>();
                                gridNeighbors.Add(line.grid1, ints1);
                            }
                            ints1.intList.Add(line.grid0);
                        }
                    }
                }
                //gridIndexs.Clear();
                if (topRange.TryGetValue(topNeighbor, out gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.intList.Count; i++)
                    {
                        int checkRight = grids[gridIndexs.intList[i]].z;
                        int checkLeft = grids[gridIndexs.intList[i]].x;
                        if (checkRight >= left && checkLeft <= right)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs.intList[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkLeft, left), math.min(checkRight, right));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(center, topNeighbor - 0.5f));
                            }

                            if (!gridNeighbors.TryGetValue(line.grid0, out var ints))
                            {
                                ints.intList = new List<int>();
                                gridNeighbors.Add(line.grid0, ints);
                            }
                            ints.intList.Add(line.grid1);

                            if (!gridNeighbors.TryGetValue(line.grid1, out var ints1))
                            {
                                ints1.intList = new List<int>();
                                gridNeighbors.Add(line.grid1, ints1);
                            }
                            ints1.intList.Add(line.grid0);
                        }
                    }
                }
            }
        }
        allCellPoints.Clear();

        Debug.Log($"消耗时间{Time.realtimeSinceStartup - timeValue}");

        DestroyImmediate(textParent.gameObject);
        var obj = new GameObject("Texts");
        textParent = obj.transform;
        for(int i = 0; i < grids.Count; i++)
        {
            Vector3 center = new Vector3((grids[i].z - grids[i].x) * 0.5f+ grids[i].x, (grids[i].w - grids[i].y) * 0.5f+ grids[i].y) * 0.08f;
            var _text = Instantiate(textMesh, center, quaternion.identity, textParent);
            _text.text = i.ToString();
        }

        //cells.Clear();
    }

    public static List<int> CellToGrid(List<int2> cells, ref Dictionary<GridLine, Vector2> gridNeighborDic)
    {
        List<int> result = new List<int>();

        Dictionary<int, List<int>> leftRange = new Dictionary<int, List<int>>();
        Dictionary<int, List<int>> rightRange = new Dictionary<int, List<int>>();
        Dictionary<int, List<int>> bottomRange = new Dictionary<int, List<int>>();
        Dictionary<int, List<int>> topRange = new Dictionary<int, List<int>>();

        HashSet<int2> allCellPoints = new HashSet<int2>();

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
                result.Add(left);
                result.Add(bottom);
                result.Add(right);
                result.Add(top);

                int gridIndex = result.Count / 4 - 1;
                if (leftRange.TryGetValue(right, out var leftList))
                {
                    leftList.Add(gridIndex);
                }
                else
                {
                    leftList = new List<int>();
                    leftList.Add(gridIndex);
                    leftRange.Add(right, leftList);
                }
                if (rightRange.TryGetValue(left, out var rightList))
                {
                    rightList.Add(gridIndex);
                }
                else
                {
                    rightList = new List<int>();
                    rightList.Add(gridIndex);
                    rightRange.Add(left, rightList);
                }
                if (bottomRange.TryGetValue(top, out var bottomList))
                {
                    bottomList.Add(gridIndex);
                }
                else
                {
                    bottomList = new List<int>();
                    bottomList.Add(gridIndex);
                    bottomRange.Add(top, bottomList);
                }
                if (topRange.TryGetValue(bottom, out var topList))
                {
                    topList.Add(gridIndex);
                }
                else
                {
                    topList = new List<int>();
                    topList.Add(gridIndex);
                    topRange.Add(bottom, topList);
                }

                int leftNeighbor = left - 1;
                int rightNeighbor = right + 1;
                int bottomNeighbor = bottom - 1;
                int topNeighbor = top + 1;
                if (leftRange.TryGetValue(leftNeighbor, out var gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.Count; i++)
                    {
                        int checkTop = result[gridIndexs[i] * 4 + 3];
                        int checkBottom = result[gridIndexs[i] * 4 + 1];
                        if (checkTop >= bottom && checkBottom <= top)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkBottom, bottom), math.min(checkTop, top));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(leftNeighbor + 0.5f, center));
                            }
                        }
                    }
                }
                if (rightRange.TryGetValue(rightNeighbor, out gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.Count; i++)
                    {
                        int checkTop = result[gridIndexs[i] * 4 + 3];
                        int checkBottom = result[gridIndexs[i] * 4 + 1];
                        if (checkTop >= bottom && checkBottom <= top)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkBottom, bottom), math.min(checkTop, top));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(rightNeighbor - 0.5f, center));
                            }
                        }
                    }
                }
                if (bottomRange.TryGetValue(bottomNeighbor, out gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.Count; i++)
                    {
                        int checkRight = result[gridIndexs[i] * 4 + 2];
                        int checkLeft = result[gridIndexs[i] * 4];
                        if (checkRight >= left && checkLeft <= right)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkLeft, left), math.min(checkRight, right));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(center, bottomNeighbor + 0.5f));
                            }
                        }
                    }
                }
                if (topRange.TryGetValue(topNeighbor, out gridIndexs))
                {
                    for (int i = 0; i < gridIndexs.Count; i++)
                    {
                        int checkRight = result[gridIndexs[i] * 4 + 2];
                        int checkLeft = result[gridIndexs[i] * 4];
                        if (checkRight >= left && checkLeft <= right)
                        {
                            GridLine line = new GridLine();
                            line.grid0 = gridIndex;
                            line.grid1 = gridIndexs[i];
                            if (!gridNeighborDic.ContainsKey(line))
                            {
                                Vector2Int crossRange = new Vector2Int(math.max(checkLeft, left), math.min(checkRight, right));
                                float center = (crossRange.x + crossRange.y) * 0.5f;
                                gridNeighborDic.Add(line, new Vector2(center, topNeighbor - 0.5f));
                            }
                        }
                    }
                }
            }
        }
        allCellPoints.Clear();
        cells.Clear();
        return result;
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < grids.Count; i++)
        {
            var grid = grids[i];
            Vector3 p0 = new Vector3(grid.x-0.5f, grid.y - 0.5f, 0)*0.08f;
            Vector3 p1 = new Vector3(grid.x - 0.5f, grid.w + 0.5f, 0) * 0.08f;
            Vector3 p2 = new Vector3(grid.z+0.5f, grid.w + 0.5f, 0) * 0.08f;
            Vector3 p3 = new Vector3(grid.z + 0.5f, grid.y - 0.5f, 0) * 0.08f;
           // Graphics.draw
           Gizmos.color = Color.yellow;

            Gizmos.DrawLine(p0, p1);
            Gizmos.DrawLine(p1, p2);
            Gizmos.DrawLine(p2, p3);
            Gizmos.DrawLine(p3, p0);


        }
    }
    private void Update()
    {



    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(TestGrid))]
public class TestGridEditor : Editor
{
    public TestGrid testGrid
    {
        get
        {
            return target as TestGrid;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("测试"))
        {
            testGrid.CellToGridInt4();
        }
        if (GUILayout.Button("地图测试"))
        {
            //testGrid.CellToGridMap();
        }
        if (GUILayout.Button("移除测试"))
        {
            testGrid.InitOutCell();
        }
        if (GUILayout.Button("除测试"))
        {
            testGrid.RemoveGridLine();
        }
    }
}

#endif

[Serializable]
public struct GridLine
{
    public int grid0, grid1;

    public static bool operator ==(GridLine a, GridLine b)
    {
        return (a.grid0 == b.grid0 && a.grid1 == b.grid1)
            || (a.grid0 == b.grid1 && a.grid1 == b.grid0);
    }

    public static bool operator !=(GridLine a, GridLine b)
    {
        return !((a.grid0 == b.grid0 && a.grid1 == b.grid1)
           || (a.grid0 == b.grid1 && a.grid1 == b.grid0));
    }

    public override bool Equals(object obj)
    {
        if (obj is GridLine)
        {
            GridLine other = (GridLine)obj;
            return (grid0 == other.grid0 && grid1 == other.grid1)
                || (grid0 == other.grid1 && grid1 == other.grid0);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return grid0.GetHashCode() ^ grid1.GetHashCode();
    }
}
