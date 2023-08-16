using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MapCell
{
    public Vector2Int coordinate;
    public bool isWalkable;

    public int parentCell;
    private int s_Cost, e_Cost;

    public int t_Cost
    {
        get
        {
            return s_Cost + e_Cost;
        }
    }

    public void CalculateDistanceCost(int s_Cost, int e_Cost)
    {
        this.s_Cost = s_Cost;
        this.e_Cost = e_Cost;
    }

    public bool CheckCellPos(Vector2Int coordinate)
    {
        return this.coordinate == coordinate;
    }

    public static bool operator ==(MapCell cell1, MapCell cell2)
    {
        return cell1.coordinate == cell2.coordinate;
    }

    public static bool operator !=(MapCell cell1, MapCell cell2)
    {
        return cell1.coordinate != cell2.coordinate;
    }
}
public delegate MapCell GetMapCell(Vector2Int coordinate);
public class FindPath
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    private static List<int[]> neighbourCoordiantes = new List<int[]>
    {
        new int[]{-1,0 },// Left
        new int[]{1,0 },// Right
        new int[]{0,1 },// Up
        new int[]{0,-1 },// Down
        new int[]{-1,-1 },// Left Down
        new int[]{-1,1 },// Left Up
        new int[]{1,-1 },// Right Down
        new int[]{1,1 },// Right Up
    };

    private static List<MapCell> GetNeighbourMapCells(MapCell nowCell, GetMapCell GetMapCell)
    {
        List<MapCell> neighbourCells = new List<MapCell>();

        foreach (var coordinate in neighbourCoordiantes)
        {
            Vector2Int neighbourCoordinate = new Vector2Int(coordinate[0] + nowCell.coordinate.x, coordinate[1] + nowCell.coordinate.y);
            MapCell neighbourCell = GetMapCell(neighbourCoordinate);

            if (neighbourCell.isWalkable)
            {
                neighbourCells.Add(neighbourCell);
            }
        }

        return neighbourCells;
    }

    public static Stack<Vector2Int> GetPath(Vector2Int startPos, Vector2Int targetPos, GetMapCell GetMapCell)
    {
        Stack<Vector2Int> nodes = new Stack<Vector2Int>();

        MapCell targetCell = GetMapCell(targetPos);
        MapCell startCell =GetMapCell(startPos);
        if (startCell.isWalkable && targetCell.isWalkable)
        {
            List<MapCell> openList = new List<MapCell>();
            List<MapCell> closeList = new List<MapCell>();

            openList.Add(startCell);

            List<Vector2Int> openCoordinateList = new List<Vector2Int>();

            while (openList.Count > 0)
            {
                MapCell nowCell = openList[0];

                openList.Remove(nowCell);
                closeList.Add(nowCell);
                if (nowCell.CheckCellPos(targetPos))
                {
                    break;
                }

                var neighbourCells = GetNeighbourMapCells(nowCell, GetMapCell);

                for (int i = 0; i < neighbourCells.Count; i++)
                {
                    var cell = neighbourCells[i];

                    if (closeList.Contains(cell) || openCoordinateList.Contains(cell.coordinate))
                    {
                        continue;
                    }

                    cell.CalculateDistanceCost(CalculateDistanceCost(cell.coordinate, startPos) * 5,
                        CalculateDistanceCost(cell.coordinate, targetPos));

                    cell.parentCell = closeList.Count - 1;

                    bool insert = false;
                    for (int j = 0; j < openList.Count; j++)
                    {
                        if (openList[j].t_Cost > cell.t_Cost)
                        {
                            openList.Insert(j, cell);
                            openCoordinateList.Add(cell.coordinate);
                            insert = true;
                            break;
                        }
                    }
                    if (!insert)
                    {
                        openList.Add(cell);
                        openCoordinateList.Add(cell.coordinate);
                    }
                }
            }

            var checkCell = closeList[closeList.Count - 1];
            nodes.Push(checkCell.coordinate);
            while (checkCell.coordinate != startPos)
            {
                int index = checkCell.parentCell;
                if (closeList.Count <= index)
                {
                    break;
                }
                checkCell = closeList[index];
                nodes.Push(checkCell.coordinate);

                if (index == 0)
                {
                    break;
                }
            }
        }

        return nodes;
    }

    private static int CalculateDistanceCost(Vector2Int aPosition, Vector2Int bPosition)
    {
        int xDistance = Mathf.Abs(aPosition.x - bPosition.x);
        int yDistance = Mathf.Abs(aPosition.y - bPosition.y);
        int remaining = Mathf.Abs(xDistance - yDistance);
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }
}