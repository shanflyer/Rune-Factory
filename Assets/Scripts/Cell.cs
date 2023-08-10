using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 


public enum FieldStatus
{
    Barren=0,
    Wet=1,
    Dry=2
}
public class Cell : IComparable<Cell>
{
    public Vector2 pos;
    public Vector2Int coordinate;
    public float cost0, cost1, cost;
    public bool isChecked = false;
    public GameObject tileMask;
    public GameObject tileObj;
    private Cell parentCell;
    public bool isRange;
    public bool isDirected;
    public bool isRangeAttacked;
    public bool isAttacked;
    public List<MyGameObject> myGameObjects;
    public MyTile tile;
    public bool isArable;
    public FieldStatus fieldStatus;
    public int CompareTo(Cell obj)
    {

        return this.cost.CompareTo(obj.cost);

    }
    public int CompareTo(Cell _cell, Vector2 centerCoorder)
    {
        return AStarTest.Distance(coordinate,centerCoorder).CompareTo(AStarTest.Distance(_cell.coordinate,centerCoorder));
    }
   
    public Cell()
    {
       
        isRange = false;
        isDirected = false;
        pos = new Vector2(0, 0);
        coordinate = new Vector2Int(0, 0);
        parentCell = this;
        tile = new MyTile();
        tileObj = null;
        myGameObjects = new List<MyGameObject>();
        isArable = false;
        fieldStatus=FieldStatus.Barren;

    }
    public Cell(Vector2 _pos,Vector2Int _coordinate)
    {
        isRange = false;
        isDirected = false;
        pos = _pos;
        coordinate = _coordinate;
        parentCell = this;
        tileObj = null;
        myGameObjects = new List<MyGameObject>();
        tile = new MyTile();
        isArable = false;
        fieldStatus = FieldStatus.Barren;
    }
    public Cell(Vector2 _pos, Vector2Int _coordinate, GameObject _tileObj)
    {
        isRange = false;
        isDirected = false;
        pos = _pos;
        coordinate = _coordinate;
        parentCell = this;
        tileObj = _tileObj;
        myGameObjects = new List<MyGameObject>();
        tile = new MyTile();
        isArable = false;
        fieldStatus = FieldStatus.Barren;
    }
    public void InitCell()
    {
        parentCell = this;
        cost = cost0 = cost1 = 0;
        isChecked = false;
        isRange = false;
        isDirected = false;
        isAttacked = false;
        isRangeAttacked = false;

        if (tileMask != null)
        {
            tileMask.SetActive(false);
        }
        SetTileColor(tile.TileColor);
    }
    public void AfterRange()
    {
        parentCell = this;
        cost = cost0 = cost1 = 0;
        isChecked = false;
    }
    public void SetCellTile(MyTile _tile)
    {
        tile = _tile;
        EditCell();
    }
    public void EditCell()
    {
        parentCell = this;
        cost = cost0 = cost1 = 0;
        isChecked = false;
        isRange = false;
        isDirected = false;
        SetTileColor(tile.TileColor);
    }
    public void SetTile(MyTile _tile)
    {
        tile = _tile;
        
    }
    public void SetTileObj(GameObject _tileObj)
    {
        tileObj = _tileObj;
    }

 

    
    public void UnHideTileMask()
    {
        if (tileMask != null)
            tileMask.SetActive(true);
            tileMask.GetComponent<SpriteRenderer>().color = new Color(0,0,0,0.65f);
    }

    public void HideTileMask()
    {
        if (tileMask != null)
            tileMask.SetActive(false);
    }
    public void SetTileMask(Color color)
    {
        if (tileMask != null)
            tileMask.GetComponent<SpriteRenderer>().color = color;
    }
    public void SetTileColor(Color color)
    {
        if (tileObj != null)
            tileObj.GetComponentInChildren<SpriteRenderer>().color = color;
    }
    public void SetTileColor()
    {
        if (tileObj != null)
            tileObj.GetComponentInChildren<SpriteRenderer>().color = tile.TileColor;
    }
    public void SetTileObjActive(bool isTileObj)
    {
        tileObj.SetActive(isTileObj);
    }
    public void SetParent(Cell parent)
    {
        parentCell = parent;
    }
    public float CostRangeValue(int barrierValueAddtion)
    {
        cost0 = parentCell.cost0 + tile.barrierValue*barrierValueAddtion;
        return cost0;
    }
    public void CostCalculation(Cell startCell, Cell goldCell,int barrierValueAddtion)
    {
        if (parentCell.coordinate != coordinate)
        {
            cost0 = parentCell.cost0 + tile.barrierValue*barrierValueAddtion;
        }
        cost1 = 5 * (Mathf.Abs(goldCell.coordinate.x - coordinate.x) + Mathf.Abs(goldCell.coordinate.y - coordinate.y) + tile.barrierValue * barrierValueAddtion);
        float dx1 = coordinate.x - goldCell.coordinate.x;
        float dy1 = coordinate.y - goldCell.coordinate.y;
        float dx2 = startCell.coordinate.x - goldCell.coordinate.x;
        float dy2 = startCell.coordinate.y - goldCell.coordinate.y;
        cost = cost0 + cost1 + Mathf.Abs(dx1 * dy2 - dx2 * dy1);
    }
    public void AddMyGameObject(MyGameObject myGameObject)
    {
        myGameObjects.Add(myGameObject);
    }
    public Cell GetParent()
    {
        return parentCell;
    }
   
}
