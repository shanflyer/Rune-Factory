using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LitJson;
using System.IO;
using System.Text;
using OldName;
[System.Serializable]
public class OldMapCellData
{
    public string mapName;
    public List<MapTile> mapTiles;
    public OldMapCellData()
    {
        mapName = "";
        mapTiles = new List<MapTile>();
    }
    public OldMapCellData(string _mapName, List<MapTile> _mapTiles)
    {
        mapName = _mapName;
        mapTiles = _mapTiles;
    }
}
[System.Serializable]
public class MapTile
{
    public int x, y;
    public string tileName;
    public MapTile()
    {
        x = 0;
        y = 0;
        tileName = "";
    }
    public MapTile(Vector2 coordinate, string _tileName)
    {
        x = (int)coordinate.x;
        y = (int)coordinate.y;
        tileName = _tileName;
    }
}
public class MapEditAction : MonoBehaviour
{
    public float cameraSpeed;
    public bool isMapEdit;
    public bool isCamreaMove;
    public bool isDisplayCoordinate;
    private bool isCharactor;
    private OldMapCellData mapCellData;
    public GameObject mapObj;
    private MapDataAction mapData;
    private Cell[] cellList;
    public List<MyTile> tiles;
    private MyTile clickTile;

    public GameObject editCanvas, normalCanvas;
  
    // Use this for initialization
    void Awake () {

        
        
        /*JsonToMapData();*/
       
        
        /*gameManager.MonsterInit();*/
    }

   
    public void InitData()
    {
        mapData = GetComponent<MapDataAction>();
        cellList = AStarTest.cellList;
        tiles = mapData.tiles;
        clickTile = new MyTile();
        JsonToMapData();
        DisplayCell();
        MapEditSwitch();
        if (isMapEdit)
        {
            editCanvas.SetActive(true);
            normalCanvas.SetActive(false);
        }
        else
        {
            editCanvas.SetActive(false);
            normalCanvas.SetActive(true);
        }
    }
    public void SetCharactorEdit(bool _isCharactor)
    {
        isCharactor = _isCharactor;
        isCamreaMove = false;
        if (isMapEdit && isCharactor)
        {
            foreach (var cell in cellList)
            {
                cell.InitCell();
            }
        }
        if (isMapEdit && !isCharactor)
        {
            foreach (var cell in cellList)
            {
                cell.EditCell();
            }
        }
    }
    public void EditCell(string tileName)
    {
        clickTile = tiles.Find(tile => tile.name == tileName);
    }
    void DisplayCell()
    {
        if (!isMapEdit)
        {
            foreach (var cell in cellList)
            {
                cell.InitCell();
            }
        }
        else
        {
            foreach (var cell in cellList)
            {
                
                cell.EditCell();
            }
        }
       
    }

    public void MapEditSwitch()
    {
        if (isMapEdit)
        {
            JsonToMapData();
        }
        
    }
    public void SelectCell()
    {
        if (!isCamreaMove && isMapEdit && !isCharactor)
        {
            Vector3 bgScreenPos = Camera.main.WorldToScreenPoint(mapObj.transform.position);
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, bgScreenPos.z);
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 clickCoordinate = AStarTest.PosToCoordinate(new Vector2(clickPos.x, clickPos.y));
            if (clickCoordinate.x != -1)
            {
                Cell clickCell = AStarTest.GetCellWithCoordinate(clickCoordinate);
                clickCell.SetCellTile(clickTile);
            }
        }
        if (!isMapEdit)
        {
            Vector3 bgScreenPos = Camera.main.WorldToScreenPoint(mapObj.transform.position);
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, bgScreenPos.z);
            Vector3 clickPos = Camera.main.ScreenToWorldPoint(mousePos);
            Vector2 clickCoordinate = AStarTest.PosToCoordinate(new Vector2(clickPos.x, clickPos.y));
            Cell clickCell = AStarTest.GetCellWithCoordinate(clickCoordinate);
            if (clickCell != null)
            {
                GameComponentData.gameData.gameManager.ClickCell(clickCell);
                GameComponentData.gameData.NpcManager.ZeroTalkAction(GameComponentData.gameData.passDataManager.nowPass,clickCoordinate);
            }
            
        }
    }
    public void MapDataToJson()
    {
        string filePath = Application.dataPath + @"/Resources/Datas/";
        List<MapTile> mapTiles = new List<MapTile>();
        foreach (var cell in cellList)
        {
            MapTile mapTile = new MapTile(cell.coordinate,cell.tile.name);
            mapTiles.Add(mapTile);
        }
        mapCellData = new OldMapCellData(SceneData.PassId.ToString(), mapTiles);
        string fileName = filePath + SceneData.PassId.ToString()+ ".json";
        string jsonStr = JsonMapper.ToJson(mapCellData);
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
        FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
        /* File.WriteAllText(FILEPATH + tileDataFileName, jsonStr, Encoding.UTF8);*/
    }
    public void JsonToMapData()
    {
        TextAsset fileText = Resources.Load<TextAsset>("Datas/" + SceneData.PassId.ToString());
        
        if (fileText==null)
        {
            Debug.LogError("No" + SceneData.PassId);
        }
        else
        {
            string jsonStr = fileText.text;
            mapCellData = new OldMapCellData();
            mapCellData = JsonMapper.ToObject<OldMapCellData>(jsonStr);
            if (tiles.Count >0)
            {
                SetMapCell();
            }
            
        }
        if (isMapEdit)
        {
            DisplayCell();
        }
    }
    void SetMapCell()
    {
        foreach (var mapTile in mapCellData.mapTiles)
        {
            Vector2 coordinate = new Vector2(mapTile.x, mapTile.y);
            string tileName = mapTile.tileName;
            Cell cellX = AStarTest.GetCellWithCoordinate(coordinate);
            MyTile tileX = tiles.Find(tile => tile.name == tileName);
            if (tileX.EnglishName == "field")
            {
                cellX.isArable = true;
            }
            cellX.SetTile(tileX);
        }
    }
    public void IsCamreaMove(Toggle isCamreaMoveToggle)
    {
        if (isCamreaMoveToggle.isOn)
        {
            /*cameraScreenPos = Camera.main.WorldToScreenPoint(CamreaObj.position);*/
           /* mos = Input.mousePosition;*/
            isCamreaMove = true;
            /*StartCoroutine("CameraMove");*/
        }
        else
        {
            isCamreaMove = false;
            /*StopCoroutine("CameraMove");*/
        }
    }
//     IEnumerator CameraMove()
//     {
//         while (true)
//         {
//             cameraScreenPos = Camera.main.WorldToScreenPoint(CamreaObj.position);
//             Vector3 mouseWorldPos0 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cameraScreenPos.z));
//             Vector3 x = mouseWorldPos0 - CamreaObj.position;
//             yield return new WaitForFixedUpdate();   
//            if (Input.GetMouseButton(0))
//            {
//                Vector3 mouseWorldPos1 = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, cameraScreenPos.z));
//                CamreaObj.position = mouseWorldPos1 - x;
//            }
//         }
//     }
    // Update is called once per frame
    void Update()
    {
        /*MapEditSwitch();*/
        if (isMapEdit)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Vector3 bgScreenPos = Camera.main.WorldToScreenPoint(mapObj.transform.position);
                Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, bgScreenPos.z);
                Vector3 clickPos = Camera.main.ScreenToWorldPoint(mousePos);
                Vector2 clickCoordinate = AStarTest.PosToCoordinate(new Vector2(clickPos.x, clickPos.y));
                if (clickCoordinate.x != -1)
                {
                    Cell clickcell = AStarTest.GetCellWithCoordinate(clickCoordinate);

                    if (clickcell.myGameObjects.Count > 0)
                    {
                        /*
                        Charactor clickCharactor =
                            GameComponentData.gameData.gameManager.Charactors.Find(
                                c => c.id == clickcell.myGameObjects[0].id);
                        */

                    }
                }
            }

        }
        else
        {

        }
    }
}
