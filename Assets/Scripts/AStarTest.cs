using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OldName;
[System.Serializable]
public struct MyVector2
{
    public int x, y;

    public MyVector2(int _x, int _y)
    {
        x = _x;
        y = _y;
    }
    public MyVector2(float _x, float _y)
    {
        x =(int) _x;
        y = (int)_y;
    }
    public static implicit operator MyVector2(Vector2 v)
    {
        return new MyVector2(v.x,v.y);
    }
    public static implicit operator Vector2Int(MyVector2 v)
    {
        return new Vector2Int(v.x, v.y);
    }
    public static implicit operator Vector2(MyVector2 v)
    {
        return new Vector2Int(v.x, v.y);
    }
}
public class AStarTest {

    private static Vector2 cellNum;
    private static Vector2 startPos;
    private static Vector2 endPos;
    public static Cell[] cellList;
    private static float cellWidth, cellHigh;
    private static List<Cell> roadCells;
    private static List<Cell> checkList;
    private static List<Cell> closedList, openList;


    public AStarTest(Vector2 _cellNum, Vector2 _startPos, Vector2 _endPos)
    {
        cellNum = _cellNum;
        startPos = _startPos;
        endPos = _endPos;
        cellList = new Cell[Mathf.RoundToInt(_cellNum.x*_cellNum.y)];
        openList = new List<Cell>();
        roadCells = new List<Cell>();
        checkList = new List<Cell>();
        closedList = new List<Cell>();
        CreatCell();
 
    }
    static void InitAStar()
    {
        openList = new List<Cell>();
        roadCells=new List<Cell>();
        checkList = new List<Cell>();
        closedList = new List<Cell>();
    }

    public static List<Cell> RunAStar(Vector2 _sp, Vector2 _gp,Charactor charactor)
    {
        InitAStar();
        CreatRoad(_sp, _gp, charactor);
        DisplayRoad(_sp, _gp);     
        return roadCells;        
    }
    public List<Cell> RunAStar2(Vector2 _sp, Vector2 _gp, Charactor charactor)
    {
        InitAStar();
        CreatRoad2(_sp, _gp, charactor);
        DisplayRoad(_sp, _gp);
        return roadCells;
    }
    public List<Cell> RunAStar3(Vector2 _sp, Vector2 _gp, Charactor charactor)
    {
        InitAStar();
        CreatRoad3(_sp, _gp, charactor);
        DisplayRoad3(_sp, _gp);
        return roadCells;
    }

    public static Cell FindCellForCoordinate(Vector2 coordinate,List<Cell> cells)
    {
        for (int i = 0; i <cells.Count ; i++)
        {
            if (cells[i].coordinate == coordinate)
            {
                return cells[i];
            }
        }
        return null;
    }
    public static List<Cell> GetZeroCellDistanceWithCenter(Vector2 centerCoordinate, int distance)
    {
        List<Cell> cells=new List<Cell>();
        for (int xi = 0; xi <= distance; xi++)
        {
            float x = xi + centerCoordinate.x;
            float y = distance - x + centerCoordinate.x + centerCoordinate.y;
            Cell cell = GetCellWithCoordinate(new Vector2(x, y));
            if (cell != null&&cell.myGameObjects.Count==0)
            {
                cells.Add(cell);
            }
        }
        for (int xi = 0; xi < distance; xi++)
        {
            float x = xi + centerCoordinate.x;
            float y = x-centerCoordinate.x+centerCoordinate.y-distance;
            Cell cell = GetCellWithCoordinate(new Vector2(x, y));
            if (cell != null && cell.myGameObjects.Count == 0)
            {
                cells.Add(cell);
            }
        }
        for (int xi = 1; xi <= distance; xi++)
        {
            float x = centerCoordinate.x -xi;
            float y = distance+x-centerCoordinate.x+centerCoordinate.y;
            Cell cell = GetCellWithCoordinate(new Vector2(x, y));
            if (cell != null && cell.myGameObjects.Count == 0)
            {
                cells.Add(cell);
            }
        }
        for (int xi = 1; xi < distance; xi++)
        {
            float x = centerCoordinate.x - xi;
            float y = centerCoordinate.x+centerCoordinate.y-x+distance;
            Cell cell = GetCellWithCoordinate(new Vector2(x, y));
            if (cell != null && cell.myGameObjects.Count == 0)
            {
                cells.Add(cell);
            }
        }
        return cells;
    }
    public static void CreatMaskTile(GameObject tile, Transform tileParent)
    {
        
        for (int i = 0; i < cellList.Length; i++)
        {
            Vector3 pos = cellList[i].pos;
            GameObject t = MonoBehaviour.Instantiate(tile, pos, Quaternion.identity) as GameObject;
            t.transform.localScale = new Vector3(cellWidth, cellHigh, 1);
            t.transform.parent = tileParent;
            t.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0.5f);
            t.SetActive(false);
            cellList[i].tileMask=t;
        }

    }
    public static void CreatTile(GameObject tile,Transform tileParent)
    {
        for (int i = 0; i < cellList.Length; i++)
        {
           Vector3 pos = cellList[i].pos;
            GameObject t = MonoBehaviour.Instantiate(tile, pos, Quaternion.identity) as GameObject;
            
            t.transform.parent = tileParent;
            t.transform.localScale=Vector3.one;
            cellList[i].SetTileObj(t);
            if (t!= null)
            {

                cellList[i].SetTileColor();
                Text coordinaText= t.GetComponentInChildren<Text>();
                coordinaText.text = cellList[i].coordinate.x + "," + cellList[i].coordinate.y;
                if (GameComponentData.gameData.mapEditAction.isDisplayCoordinate)
                {
                    coordinaText.enabled = true;
                    t.GetComponentInChildren<SpriteRenderer>().enabled = true;
                    t.GetComponentInChildren<SpriteRenderer>().color = cellList[i].tile.TileColor;

                }
                else
                {
                    coordinaText.enabled = false;
                    t.GetComponentInChildren<SpriteRenderer>().enabled = false;
                }
                

            }
        }
       
    }
    public static void ClearCellCharactor()
    {
        for (int i = 0; i < cellList.Length; i++)
        {
            cellList[i].myGameObjects.Clear();
        }
    }
    
    public static void HideTileMask()
    {
        for (int i = 0; i < cellList.Length; i++)
        {
            cellList[i].HideTileMask();
        }
    }
    public static void HideCell()
    {
        for (int i = 0; i < cellList.Length; i++)
        {
            cellList[i].tileMask.SetActive(false);
           cellList[i].SetTileColor(new Color(0, 0, 0, 0));
        }
    }
    public static void UnHideTileMask()
    {
        for (int i = 0; i < cellList.Length; i++)
        {
            cellList[i].UnHideTileMask();
        }
    }
    public static List<Cell> GetCellsWithCenter(Vector2 centerCoordinate, int distance)
    {
        List<Cell> cells=new List<Cell>();
        for (int xi = 0; xi <= distance; xi++)
        {
            for (int yi = 0; yi <= distance - xi; yi++)
            {
                float x = xi + centerCoordinate.x;
                float y = centerCoordinate.y+yi;
                Cell cell = GetCellWithCoordinate(new Vector2(x, y));
                if (cell != null )
                {
                    cells.Add(cell);
                }
            }
            
        }
        for (int xi = 0; xi < distance; xi++)
        {
            for (int yi = -1; yi >= -distance + xi; yi--)
            {
                float x = xi + centerCoordinate.x;
                float y = centerCoordinate.y + yi;
                Cell cell = GetCellWithCoordinate(new Vector2(x, y));
                if (cell != null )
                {
                    cells.Add(cell);
                }
            }
               
        }
        for (int xi = 0; xi <= distance; xi++)
        {
            for (int yi = 0; yi <= distance - xi; yi++)
            {
                float x = -xi + centerCoordinate.x;
                float y = centerCoordinate.y + yi;
                Cell cell = GetCellWithCoordinate(new Vector2(x, y));
                if (cell != null)
                {
                    cells.Add(cell);
                }
            }

        }
        for (int xi = 0; xi < distance; xi++)
        {
            for (int yi = -1; yi >= -distance + xi; yi--)
            {
                float x = -xi + centerCoordinate.x;
                float y = centerCoordinate.y + yi;
                Cell cell = GetCellWithCoordinate(new Vector2(x, y));
                if (cell != null)
                {
                    cells.Add(cell);
                }
            }

        }
        return cells;
    }
    public static Cell GetCellWithCoordinate(Vector2 coordinate)
    {
        if (coordinate.x < 0 || coordinate.y < 0 || coordinate.x > cellNum.x-1 || coordinate.y > cellNum.y-1)
        {
            return null;
        }
        else
        {
            int index =Mathf.RoundToInt(coordinate.x * cellNum.y + coordinate.y);
            return cellList[index];
        }
    }

    public static void SetCellCharactor(Vector2 coordinate)
    {
        Cell cell = GetCellWithCoordinate(coordinate);
        Debug.Log("aster.cell:"+cell.myGameObjects.Count);
        cell.myGameObjects.Clear();
    }
    public static  int Distance(Cell cell0, Cell cell1)
    {
        int distance =Mathf.RoundToInt(Mathf.Abs(cell0.coordinate.x-cell1.coordinate.x) + Mathf.Abs(cell0.coordinate.y-cell1.coordinate.y));
        return distance;
    }
    public static  int Distance(Vector2 cell0, Vector2 cell1)
    {
        int distance = Mathf.RoundToInt(Mathf.Abs(cell0.x - cell1.x) + Mathf.Abs(cell0.y - cell1.y));
        return distance;
    }
    public static  int Distance(Charactor charactor0, Charactor charactor1)
    {
        int distance = Mathf.RoundToInt(Mathf.Abs(charactor0.coordinate.x - charactor1.coordinate.x) + Mathf.Abs(charactor0.coordinate.y - charactor1.coordinate.y));
        return distance;
    }
 
    public static Vector2Int PosToCoordinate(Vector2 pos)
    {
        int x = Mathf.FloorToInt((pos.x - startPos.x) / cellWidth);
        int y = Mathf.FloorToInt((pos.y - startPos.y) / cellHigh);
        if (x < 0 || y < 0 || x > (cellNum.x-1) || y > (cellNum.y-1))
        {
            return new Vector2Int(-1, -1);
        }
        return new Vector2Int(x, y);
    }
    public static Vector2 CoordinateToPos(Vector2 coordinate)
    {
        float x = (coordinate.x + 0.5f) * cellWidth+startPos.x;
        float y = (coordinate.y + 0.5f) * cellHigh+startPos.y;
        return new Vector2(x, y);
    }
    void CreatCell()
    {
        cellWidth = (endPos.x - startPos.x) / cellNum.x;
        cellHigh = (endPos.y - startPos.y) / cellNum.y;
        cellList = new Cell[Mathf.RoundToInt(cellNum.x*cellNum.y)];
        for (int xi = 0; xi < cellNum.x; xi++)
        {
            for (int yi = 0; yi < cellNum.y; yi++)
            {
                Vector2Int coordinate=new Vector2Int(xi,yi);
                Cell c = new Cell(CoordinateToPos(coordinate), coordinate);
                cellList[Mathf.RoundToInt(xi * cellNum.y + yi)] = c;
            }
        }

    }
    static List<Cell> GetAdjacentCell(Cell cell)
    {
        List<Cell> adjacentCell = new List<Cell>();
        Vector2 coordinate1 = new Vector2(cell.coordinate.x - 1, cell.coordinate.y);
        Vector2 coordinate2 = new Vector2(cell.coordinate.x + 1, cell.coordinate.y);
        Vector2 coordinate3 = new Vector2(cell.coordinate.x, cell.coordinate.y - 1);
        Vector2 coordinate4 = new Vector2(cell.coordinate.x, cell.coordinate.y + 1);
        Cell cell1=GetCellWithCoordinate(coordinate1);
        Cell cell2=GetCellWithCoordinate(coordinate2);
        Cell cell3=GetCellWithCoordinate(coordinate3);
        Cell cell4=GetCellWithCoordinate(coordinate4);
        if (cell1!= null)
        {
            adjacentCell.Add(cell1);
        }
        if (cell2 != null)
        {
            adjacentCell.Add(cell2);
        }
        if (cell3 != null)
        {
            adjacentCell.Add(cell3);
        }
        if (cell4 != null)
        {
            adjacentCell.Add(cell4);
        }
        return adjacentCell;
    }
    public static List<Cell> CreatRange(Cell centerCell, int range, Charactor charactor)
    {
        List<Cell> rangeCells = new List<Cell>();
        List<Cell> checkedCells = new List<Cell>();
        List<Cell> newCells = new List<Cell>();
        /*centerCell.isRange = true;*/
        rangeCells.Add(centerCell);
        checkedCells.Add(centerCell);
        while (true)
        {
            foreach (var checkedcell in checkedCells)
            {
                List<Cell> cells = GetAdjacentCell(checkedcell);
                foreach (var cell in cells)
                {
                    int tilebarrierValue;
                    int addtion;
                    ArmData armData = CharactorDataAction.FindArmDataForArm(charactor.professionData.arm);
                    TileAddition tileAddition = armData.FindTileAdditionForTileType(cell.tile.tileType);
                    if (tileAddition != null)
                    {
                        addtion = tileAddition.barrierMultiply;
                    }
                    else
                    {
                        addtion = 1;
                    }
                    tilebarrierValue = cell.tile.barrierValue * addtion;

                    if (cell.GetParent() != cell)
                    {
                        Cell oldParent = cell.GetParent();
                        float oldCost0 = cell.cost0;
                        cell.SetParent(checkedcell);
                        if (cell.CostRangeValue(addtion) < oldCost0)
                        {
                            newCells.Add(cell);
                        }
                        else
                        {
                            cell.SetParent(oldParent);
                            cell.cost0 = oldCost0;
                        }
                    }
                    else
                    {
                        cell.SetParent(checkedcell);
                        if (cell.myGameObjects.Count > 0)
                        {
                            Charactor charactor1 = GameComponentData.gameData.gameManager.Charactors.Find(c => c.id == cell.myGameObjects[0].id);
                            if (charactor1 != null)
                            {
                                rangeCells.Add(cell);
                                newCells.Add(cell);
                            }
                            else
                            {
                                cell.SetParent(cell);
                                cell.cost0 = 0;
                            }
                        }
                        else if (cell.CostRangeValue(addtion) <= range && tilebarrierValue != 0)
                        {
                            rangeCells.Add(cell);
                            newCells.Add(cell);
                        }
                        else
                        {
                            cell.SetParent(cell);
                            cell.cost0 = 0;
                        }

                    }
                }
            }
            checkedCells.Clear();
            if (newCells.Count > 0)
            {
                foreach (var cell in newCells)
                {
                    checkedCells.Add(cell);
                }
            }
            else
            {
                break;
            }
            newCells.Clear();
        }
        foreach (var c in rangeCells)
        {
            c.AfterRange();
        }
        return rangeCells;
    }
   
    static void DisplayRoad(Vector2 _sp, Vector2 _gp)
    {
        
        Vector2 sc =_sp;
        Vector2 gc =_gp;
        roadCells.Clear();
        if (sc == gc)
        {
            roadCells.Add(closedList.Find(c => c.coordinate == gc));
        }
        else
        {
            if (closedList.Exists(c => c.coordinate == gc))
            {
                roadCells.Add(closedList.Find(c => c.coordinate == gc));
                while (true)
                {
                    if (roadCells[roadCells.Count - 1].coordinate == sc)
                    {
                       break;
                    }
                    else
                    {
                        if (roadCells[roadCells.Count - 1].GetParent() == null ||
                            roadCells[roadCells.Count - 1].GetParent() == roadCells[roadCells.Count - 1])
                        {
                            break;
                        }
                        if (roadCells[roadCells.Count - 1].GetParent() != roadCells[roadCells.Count - 1])
                        {
                            roadCells.Add(roadCells[roadCells.Count - 1].GetParent());
                        }
                        else
                        {
                            break;
                        }
                       
                    }
                   
                }
            }
            else
            {
                roadCells = null;
            }
        }
        
    }
    void DisplayRoad3(Vector2 _sp, Vector2 _gp)
    {

        Vector2 sc = _sp;
        Vector2 gc = _gp;
        roadCells.Clear();
        if (sc == gc)
        {
            roadCells.Add(closedList.Find(c => c.coordinate == gc));
        }
        else
        {
            if (closedList.Exists(c => c.coordinate == gc))
            {
                roadCells.Add(closedList.Find(c => c.coordinate == gc));
                while (true)
                {
                    if (roadCells[roadCells.Count - 1].coordinate == sc)
                    {
                        break;
                    }
                    else
                    {
                        
                        roadCells.Add(roadCells[roadCells.Count - 1].GetParent());
                    }

                }
            }
            else
            {
                Debug.Log("!!!!!!");
            }
        }

    }
    static void CreatRoad(Vector2 startCoordinate, Vector2 goldCoordinate, Charactor charactor)
    {
        Cell startCell = AStarTest.GetCellWithCoordinate(startCoordinate); 
        Cell goldCell = AStarTest.GetCellWithCoordinate(goldCoordinate);
        for (int i = 0; i < cellList.Length; i++)
        {
            openList.Add(cellList[i]);
        }
        closedList.Add(startCell);
        openList.Remove(startCell);
        /*startCell.CostCalculation(startCell, goldCell,1);*/
        checkList.Add(startCell);
        while (true)
        {            
            if (checkList.Count > 0&&openList.Count>0)
            {
                Cell XCell = checkList[0];
                float x0 = XCell.coordinate.x;
                float y0 = XCell.coordinate.y;
                float x1 = XCell.coordinate.x - 1;
                float x2 = XCell.coordinate.x + 1;
                float y1 = XCell.coordinate.y - 1;
                float y2 = XCell.coordinate.y + 1;
                if (y1 >= 0)
                {
                    Cell child1 = FindCellForCoordinate(new Vector2(x0,y1),openList);
                    if (child1!=null)
                    {
                        int tilebarrierValue;
                        int addtion;
                        //ArmData armData = CharactorDataAction.FindArmDataForArm(charactor.professionData.arm);
                        //TileAddition tileAddition = armData.FindTileAdditionForTileType(child1.tile.tileType);
                       /* if (tileAddition!=null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion=1;
                        }*/
                        addtion = 1;
                        tilebarrierValue = child1.tile.barrierValue * addtion;
                        if (child1.myGameObjects.Count > 0)
                        {
                            Charactor charactor1 =GameComponentData.gameData.gameManager.Charactors.Find(c=>c.id==child1.myGameObjects[0].id);
                            if (charactor1 !=null)
                            {
                                child1.SetParent(XCell);
                                child1.CostCalculation(startCell, goldCell, addtion);
                                checkList.Add(child1);
                            }
                        }
                        if (tilebarrierValue != 0 &&
                            (child1.myGameObjects.Count == 0 || child1.myGameObjects[0].coordinate == goldCoordinate))
                        {
                            child1.SetParent(XCell);
                            child1.CostCalculation(startCell, goldCell, addtion);
                            checkList.Add(child1);
                        }
                        
                        openList.Remove(child1);
                        closedList.Add(child1);
                        openList.Remove(child1);
                        if (child1 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (y2 <= cellNum.y)
                {

                    Cell child2 = FindCellForCoordinate(new Vector2(x0, y2), openList);
                    if (child2!=null)
                    {
                        int tilebarrierValue;
                        int addtion=1;
                        /*ArmData armData = CharactorDataAction.FindArmDataForArm(charactor.professionData.arm);
                        TileAddition tileAddition = armData.FindTileAdditionForTileType(child2.tile.tileType);
                        if (tileAddition != null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion = 1;
                        }*/

                        tilebarrierValue = child2.tile.barrierValue * addtion;
                        
                       if(tilebarrierValue != 0)
                        {
                            if (child2.myGameObjects.Count > 0)
                            {
                                Charactor charactor2 =
                                    GameComponentData.gameData.gameManager.Charactors.Find(
                                        c => c.id == child2.myGameObjects[0].id);
                                if (charactor2 != null )
                                {
                                    child2.SetParent(XCell);
                                    child2.CostCalculation(startCell, goldCell, addtion);
                                    checkList.Add(child2);
                                }
                                if (child2.coordinate == goldCoordinate)
                                {
                                    child2.SetParent(XCell);
                                    child2.CostCalculation(startCell, goldCell, addtion);
                                    checkList.Add(child2);
                                }
                            }
                            else
                            {
                                child2.SetParent(XCell);
                                child2.CostCalculation(startCell, goldCell, addtion);
                                checkList.Add(child2);
                            }

                           
                        }
                        
                        openList.Remove(child2);
                        closedList.Add(child2);
                        openList.Remove(child2);
                        if (child2 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (x2 <= cellNum.x)
                {
                    Cell child3 = FindCellForCoordinate(new Vector2(x2, y0), openList);
                    if (child3!=null)
                    {
                        int tilebarrierValue;
                        int addtion=1;
                        /*
                        ArmData armData = CharactorDataAction.FindArmDataForArm(charactor.professionData.arm);
                        TileAddition tileAddition = armData.FindTileAdditionForTileType(child3.tile.tileType);
                        if (tileAddition != null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion = 1;
                        }*/
                        tilebarrierValue = child3.tile.barrierValue * addtion;
                        if (tilebarrierValue != 0)
                        {
                            if (child3.myGameObjects.Count > 0)
                            {
                                Charactor charactor3 =
                                    GameComponentData.gameData.gameManager.Charactors.Find(
                                        c => c.id == child3.myGameObjects[0].id);
                                if (charactor3 != null )
                                {
                                    child3.SetParent(XCell);
                                    child3.CostCalculation(startCell, goldCell, addtion);
                                    checkList.Add(child3);
                                }
                                if (child3.coordinate == goldCoordinate)
                                {
                                    child3.SetParent(XCell);
                                    child3.CostCalculation(startCell, goldCell, addtion);
                                    checkList.Add(child3);
                                }
                            }
                            else
                            {
                                child3.SetParent(XCell);
                                child3.CostCalculation(startCell, goldCell, addtion);
                                checkList.Add(child3);
                            }


                        }
                        openList.Remove(child3);
                        closedList.Add(child3);
                        openList.Remove(child3);
                        if (child3 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (x1 >=0)
                {
                    Cell child4 = FindCellForCoordinate(new Vector2(x1, y0), openList);
                    if (child4!=null)
                    {
                        int tilebarrierValue;
                        int addtion=1;
                        /*
                        ArmData armData = CharactorDataAction.FindArmDataForArm(charactor.professionData.arm);
                        TileAddition tileAddition = armData.FindTileAdditionForTileType(child4.tile.tileType);
                        if (tileAddition != null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion = 1;
                        }*/
                        tilebarrierValue = child4.tile.barrierValue * addtion;
                        if (tilebarrierValue != 0)
                        {
                            if (child4.myGameObjects.Count > 0)
                            {
                                Charactor charactor4 =
                                    GameComponentData.gameData.gameManager.Charactors.Find(
                                        c => c.id == child4.myGameObjects[0].id);
                                if (charactor4 != null)
                                {
                                    child4.SetParent(XCell);
                                    child4.CostCalculation(startCell, goldCell, addtion);
                                    checkList.Add(child4);
                                }
                                if (child4.coordinate == goldCoordinate)
                                {
                                    child4.SetParent(XCell);
                                    child4.CostCalculation(startCell, goldCell, addtion);
                                    checkList.Add(child4);
                                }
                            }
                            else
                            {
                                child4.SetParent(XCell);
                                child4.CostCalculation(startCell, goldCell, addtion);
                                checkList.Add(child4);
                            }


                        }
                        openList.Remove(child4);
                        closedList.Add(child4);
                        openList.Remove(child4);
                        if (child4 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (checkList.Contains(goldCell))
                {
                    break;
                }
                checkList.Remove(XCell);
                checkList.Sort((c1, c2) => c1.CompareTo(c2));
                checkList.Reverse();
                checkList.Sort(); 
                if (closedList.Count>400)
                {

                    break;
                }
            }
            else
            {
                break;
            }
        }
    }
    void CreatRoad2(Vector2 startCoordinate, Vector2 goldCoordinate, Charactor charactor)
    {
        Cell startCell = GetCellWithCoordinate(startCoordinate);
        Cell goldCell = GetCellWithCoordinate(goldCoordinate);
        for (int i = 0; i < cellList.Length; i++)
        {
            openList.Add(cellList[i]);
        }
        closedList.Add(startCell);
        openList.Remove(startCell);
        /*startCell.CostCalculation(startCell, goldCell,1);*/
        checkList.Add(startCell);
        while (true)
        {
            if (checkList.Count > 0 && openList.Count > 0)
            {
                Cell XCell = checkList[0];
                float x0 = XCell.coordinate.x;
                float y0 = XCell.coordinate.y;
                float x1 = XCell.coordinate.x - 1;
                float x2 = XCell.coordinate.x + 1;
                float y1 = XCell.coordinate.y - 1;
                float y2 = XCell.coordinate.y + 1;
                if (y1 >= 0)
                {
                    Cell child1 = FindCellForCoordinate(new Vector2(x0, y1), openList);
                    if (child1 != null)
                    {
                        int tilebarrierValue;
                        int addtion;
                        ArmData armData = CharactorDataAction.FindArmDataForArm(charactor.professionData.arm);
                        TileAddition tileAddition = armData.FindTileAdditionForTileType(child1.tile.tileType);
                        if (tileAddition != null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion = 1;
                        }
                        tilebarrierValue = child1.tile.barrierValue * addtion;
                        if (tilebarrierValue != 0 ||
                            charactor.AttackCells.Exists(ac=>ac==child1))
                        {
                            child1.SetParent(XCell);
                            child1.CostCalculation(startCell, goldCell, addtion);
                            checkList.Add(child1);
                        }
                        openList.Remove(child1);
                        closedList.Add(child1);
                        openList.Remove(child1);
                        if (child1 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (y2 <= cellNum.y)
                {
                    Cell child2 = FindCellForCoordinate(new Vector2(x0, y2), openList);
                    if (child2 != null)
                    {
                        int tilebarrierValue;
                        int addtion;
                        ArmData armData = CharactorDataAction.FindArmDataForArm(charactor.professionData.arm);
                        TileAddition tileAddition = armData.FindTileAdditionForTileType(child2.tile.tileType);
                        if (tileAddition != null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion = 1;
                        }
                        tilebarrierValue = child2.tile.barrierValue * addtion;
                        if (tilebarrierValue != 0 ||
                            charactor.AttackCells.Exists(ac => ac == child2))
                        {
                            child2.SetParent(XCell);
                            child2.CostCalculation(startCell, goldCell, addtion);
                            checkList.Add(child2);
                        }
                        openList.Remove(child2);
                        closedList.Add(child2);
                        openList.Remove(child2);
                        if (child2 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (x2 <= cellNum.x)
                {
                    Cell child3 = FindCellForCoordinate(new Vector2(x2, y0), openList);
                    if (child3 != null)
                    {
                        int tilebarrierValue;
                        int addtion;
                        ArmData armData = CharactorDataAction.FindArmDataForArm(charactor.professionData.arm);
                        TileAddition tileAddition = armData.FindTileAdditionForTileType(child3.tile.tileType);
                        if (tileAddition != null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion = 1;
                        }
                        tilebarrierValue = child3.tile.barrierValue * addtion;
                        if (tilebarrierValue != 0 ||
                           charactor.AttackCells.Exists(ac => ac == child3))
                        {
                            child3.SetParent(XCell);
                            child3.CostCalculation(startCell, goldCell, addtion);
                            checkList.Add(child3);
                        }
                        openList.Remove(child3);
                        closedList.Add(child3);
                        openList.Remove(child3);
                        if (child3 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (x1 >= 0)
                {
                    Cell child4 = FindCellForCoordinate(new Vector2(x1, y0), openList);
                    if (child4 != null)
                    {
                        int tilebarrierValue;
                        int addtion;
                        ArmData armData = CharactorDataAction.FindArmDataForArm(charactor.professionData.arm);
                        TileAddition tileAddition = armData.FindTileAdditionForTileType(child4.tile.tileType);
                        if (tileAddition != null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion = 1;
                        }
                        tilebarrierValue = child4.tile.barrierValue * addtion;
                        if (tilebarrierValue != 0 ||
                            charactor.AttackCells.Exists(ac => ac == child4))
                        {
                            child4.SetParent(XCell);
                            child4.CostCalculation(startCell, goldCell, addtion);
                            checkList.Add(child4);
                        }
                        openList.Remove(child4);
                        closedList.Add(child4);
                        openList.Remove(child4);
                        if (child4 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (checkList.Contains(goldCell))
                {
                    break;
                }
                checkList.Remove(XCell);
                checkList.Sort((c1, c2) => c1.CompareTo(c2));
                checkList.Reverse();
                checkList.Sort();
                if (closedList.Count > 400)
                {

                    break;
                }
            }
            else
            {
                Debug.Log("NoWay!!!");
                break;
            }
        }
    }
    void CreatRoad3(Vector2 startCoordinate, Vector2 goldCoordinate, Charactor charactor)
    {
        Cell startCell =GetCellWithCoordinate(startCoordinate);
        Cell goldCell = GetCellWithCoordinate(goldCoordinate);
        for (int i = 0; i < cellList.Length; i++)
        {
            openList.Add(cellList[i]);
        }
        closedList.Add(startCell);
        openList.Remove(startCell);
        /*startCell.CostCalculation(startCell, goldCell,1);*/
        checkList.Add(startCell);
        while (true)
        {
            if (checkList.Count > 0 && openList.Count > 0)
            {
                Cell XCell = checkList[0];
                float x0 = XCell.coordinate.x;
                float y0 = XCell.coordinate.y;
                float x1 = XCell.coordinate.x - 1;
                float x2 = XCell.coordinate.x + 1;
                float y1 = XCell.coordinate.y - 1;
                float y2 = XCell.coordinate.y + 1;
                if (y1 >= 0)
                {
                    Cell child1 = FindCellForCoordinate(new Vector2(x0, y1), openList);
                    if (child1 != null)
                    {
                        int tilebarrierValue;
                        int addtion;
                        ArmData armData = CharactorDataAction.FindArmDataForArm( charactor.professionData.arm);
                        TileAddition tileAddition = armData.FindTileAdditionForTileType(child1.tile.tileType);
                        if (tileAddition != null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion = 1;
                        }
                        tilebarrierValue = child1.tile.barrierValue * addtion;
                        if (tilebarrierValue != 0)
                        {
                            child1.SetParent(XCell);
                            child1.CostCalculation(startCell, goldCell, addtion);
                            checkList.Add(child1);
                        }
                        openList.Remove(child1);
                        closedList.Add(child1);
                        openList.Remove(child1);
                        if (child1 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (y2 <= cellNum.y)
                {
                    Cell child2 = FindCellForCoordinate(new Vector2(x0, y2), openList);
                    if (child2 != null)
                    {
                        int tilebarrierValue;
                        int addtion;
                        ArmData armData = CharactorDataAction.FindArmDataForArm( charactor.professionData.arm);
                        TileAddition tileAddition = armData.FindTileAdditionForTileType(child2.tile.tileType);
                        if (tileAddition != null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion = 1;
                        }
                        tilebarrierValue = child2.tile.barrierValue * addtion;
                        if (tilebarrierValue != 0)
                        {
                            child2.SetParent(XCell);
                            child2.CostCalculation(startCell, goldCell, addtion);
                            checkList.Add(child2);
                        }
                        openList.Remove(child2);
                        closedList.Add(child2);
                        openList.Remove(child2);
                        if (child2 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (x2 <= cellNum.x)
                {
                    Cell child3 = FindCellForCoordinate(new Vector2(x2, y0), openList);
                    if (child3 != null)
                    {
                        int tilebarrierValue;
                        int addtion;
                        ArmData armData = CharactorDataAction.FindArmDataForArm( charactor.professionData.arm);
                        TileAddition tileAddition = armData.FindTileAdditionForTileType(child3.tile.tileType);
                        if (tileAddition != null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion = 1;
                        }
                        tilebarrierValue = child3.tile.barrierValue * addtion;
                        if (tilebarrierValue != 0)
                        {
                            child3.SetParent(XCell);
                            child3.CostCalculation(startCell, goldCell, addtion);
                            checkList.Add(child3);
                        }
                        openList.Remove(child3);
                        closedList.Add(child3);
                        openList.Remove(child3);
                        if (child3 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (x1 >= 0)
                {
                    Cell child4 = FindCellForCoordinate(new Vector2(x1, y0), openList);
                    if (child4 != null)
                    {
                        int tilebarrierValue;
                        int addtion;
                        ArmData armData = CharactorDataAction.FindArmDataForArm( charactor.professionData.arm);
                        TileAddition tileAddition = armData.FindTileAdditionForTileType(child4.tile.tileType);
                        if (tileAddition != null)
                        {
                            addtion = tileAddition.barrierMultiply;
                        }
                        else
                        {
                            addtion = 1;
                        }
                        tilebarrierValue = child4.tile.barrierValue * addtion;
                        if (tilebarrierValue != 0)
                        {
                            child4.SetParent(XCell);
                            child4.CostCalculation(startCell, goldCell, addtion);
                            checkList.Add(child4);
                        }
                        openList.Remove(child4);
                        closedList.Add(child4);
                        openList.Remove(child4);
                        if (child4 == goldCell)
                        {
                            break;
                        }
                    }
                }
                if (checkList.Contains(goldCell))
                {
                    break;
                }
                checkList.Remove(XCell);
                checkList.Sort((c1, c2) => c1.CompareTo(c2));
                checkList.Reverse();
                checkList.Sort();
            }
            else
            {
                Debug.Log("NoWay!!!");
                break;
            }
        }
    }
}
