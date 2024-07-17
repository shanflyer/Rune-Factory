#if UNITY_EDITOR
using System.Collections.Generic;
using System.Threading;
using TMPro;
using TreeEditor;
using Unity.Core;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class MapInstanceEditor : MonoBehaviour
{

    private MapRoomData mapRoomData;
    private Transform groundParent;
    private Transform itemParent;
    private Transform areaParent;

    private Tilemap tilemap;
    [SerializeField]
    private bool hideTilemap;

    [SerializeField]
    private bool displayCoordinate;

    public static Dictionary<int, MapItemData> mapItemDatas;

    private static string defaultGroundPath = "Assets/Resources/Prefabs/Other/DefaultGround.prefab";

    private static string prefabPath = "Assets/Resources/Prefabs/Ground/";
    private static string defaultMaterial = "Assets/Editor/Source/Default.mat";
    private const string areaPrefabPath = "Assets/Prefabs/BehaviorArea.prefab";

    public static GameObject areaPrefab
    {
        get
        {
            if (_areaPrefab == null)
            {
                _areaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(areaPrefabPath);
            }
            return _areaPrefab;
        }
    }
    private static GameObject _areaPrefab;
    public static GameObject defaultGround
    {
        get
        {
            if (_defaultGround == null)
            {
                _defaultGround = AssetDatabase.LoadAssetAtPath<GameObject>(defaultGroundPath);
            }
            return _defaultGround;
        }
    }

    private static GameObject _defaultGround;

    private static Material material
    {
        get
        {
            if (_material == null)
            {
                _material = AssetDatabase.LoadAssetAtPath<Material>(defaultMaterial);
            }
            return _material;
        }
    }
    private static Material _material;
    public static TileBase defaultTile
    {
        get
        {
            if (_defaultTile == null)
            {
                _defaultTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/TileMap/Tiles/Event/default.asset");
            }
            return _defaultTile;
        }
    }
    public static TileBase walkTile
    {
        get
        {
            if (_walkTile == null)
            {
                _walkTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/TileMap/Tiles/Event/1.asset");
            }
            return _walkTile;
        }
    }

    public static TileBase barrierTile
    {
        get
        {
            if (_barrierTile == null)
            {
                _barrierTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/TileMap/Tiles/Event/0.asset");
            }
            return _barrierTile;
        }
    }

    private static TileBase _walkTile,_defaultTile;
    private static TileBase _barrierTile;
    public int id;

    List<TilemapRenderer> tilemapRenderers = new List<TilemapRenderer>();

    private GameObject coordinateDisplayParent;
    private TextMeshPro editorCoordinate
    {
        get
        {
            if (_editorCoordinate == null)
            {
                _editorCoordinate= AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/Source/Text.prefab").GetComponent<TextMeshPro>();
            }
            return _editorCoordinate;
        }
    }
    private TextMeshPro _editorCoordinate;

    public WorldMap GetWorldMap()
    {
        WorldMap worldMap = new WorldMap
        {
           // map = mapRoomData.name,
            mapRoomData=mapRoomData,
            id = id,
            coordinate = new int3(this.coordinate.x, this.coordinate.y, (int)transform.localPosition.z)
        };
        return worldMap;
    }
    public void InitData(MapRoomData mapRoomData, int id, bool hideTilemap = false)
    {
        this.mapRoomData = mapRoomData;
        this.id = id;
        gameObject.name = mapRoomData.roomName;

        tilemapRenderers.Clear();

        if (groundParent == null)
        {
            var groundParentObj = transform.Find("GroundParent");
            if (groundParentObj == null)
            {
                groundParentObj = new GameObject("GroundParent").transform;
                groundParentObj.transform.SetParent(transform, false); 
                groundParent = groundParentObj.transform;
                //roundParent.localPosition = new Vector3(GameCommon.cellSize, GameCommon.cellSize);

                GameObject itemParentObj = new GameObject("ItemParent");
                itemParentObj.transform.SetParent(transform, false);
                itemParentObj.transform.localPosition = new Vector3(0, 0, 0);
                itemParent = itemParentObj.transform;

                GameObject Grid = new GameObject("Grid");
                Grid.transform.SetParent(transform, false);

                GameObject MapTile = new GameObject("MapTile");
                MapTile.transform.SetParent(Grid.transform, false);

                coordinateDisplayParent = new GameObject("CoordinateDisplay");
                coordinateDisplayParent.transform.SetParent(transform, false);

                var grid = Grid.AddComponent<Grid>();
                tilemap = MapTile.AddComponent<Tilemap>();
                var tilemapRenderer = MapTile.AddComponent<TilemapRenderer>();
                tilemap.color = new Color(1, 1, 1, 0.5f);
                tilemapRenderers.Add(tilemapRenderer);
                tilemapRenderer.sharedMaterial = material;

                grid.cellSize = new Vector3(GameCommon.cellWidth, GameCommon.cellHigh, 0);
                tilemapRenderer.enabled = !hideTilemap;

                GameObject areaParentObj = new GameObject("AreaParent");
                areaParentObj.transform.SetParent(transform, false);
                areaParent = areaParentObj.transform;
            }
            InitMapObj();
            InitMapTile();
            InitMapArea();
        }
    }
    public bool CheckPos(ref int2 clickCoordinate)
    {
        var _clikCoordinate = clickCoordinate - coordinate;
        if (mapRoomData.CheckBoundary(_clikCoordinate))
        {
            clickCoordinate = _clikCoordinate;
            return true;
        }

        return false;
    }
    public Vector3 GetMapPos(int2 coordinate)
    {
        Vector3 pos = GameCommon.GetMapPos(coordinate);
        return pos + transform.localPosition;
    }
    public Vector3 GetMapPos(Vector2Int coordinate)
    {
        Vector3 pos = GameCommon.GetMapPos(coordinate);
        return pos + transform.localPosition;
    }
    private void InitMapObj(bool hideTilemap = false)
    {
        if (mapRoomData != null)
        { 
            GameObject mapObj = mapRoomData.mapObj ? (GameObject)PrefabUtility.InstantiatePrefab(mapRoomData.mapObj) :
                Instantiate(defaultGround);  
            mapObj.transform.SetParent(groundParent, false); 
            mapObj.name = mapRoomData.mapObj ? mapRoomData.mapObj.name : mapRoomData.roomName;

            foreach (var item in mapRoomData.mapItems)
            {
                MapItemData itemData;
                if (mapItemDatas.TryGetValue(item.id, out itemData))
                {
                    if (itemData.itemObj)
                    {
                        GameObject itemObj = (GameObject)PrefabUtility.InstantiatePrefab(itemData.itemObj);
                        Transform itemInstance = new GameObject(itemData.itemName).transform;
                        itemObj.transform.SetParent(itemInstance, false);
                        itemObj.transform.localPosition = Vector3.zero;

                        GameObject Grid = new GameObject("Grid");
                        Grid.transform.SetParent(transform, false);  

                        GameObject ItemTile = new GameObject("ItemTile");
                        ItemTile.transform.SetParent(Grid.transform, false);

                        Grid.transform.localPosition = new Vector3(-GameCommon.cellSize, -GameCommon.cellSize, 0);
                        Grid.transform.localPosition = new Vector3(-GameCommon.cellSize, -GameCommon.cellSize, 0);

                        var grid = Grid.AddComponent<Grid>();
                        var tilemap = ItemTile.AddComponent<Tilemap>();
                        var tilemapRenderer = ItemTile.AddComponent<TilemapRenderer>(); 
                        grid.cellSize = new Vector3(GameCommon.cellWidth, GameCommon.cellHigh, 0);
                        tilemap.color = new Color(1, 1, 1, 0.5f);
                        Grid.transform.SetParent(itemInstance, false);
                        //Grid.transform.localPosition = new Vector3(-GameCommon.cellSize, -GameCommon.cellSize,0);
                        tilemapRenderer.enabled = !hideTilemap;
                        tilemapRenderer.sharedMaterial = material;
                        tilemapRenderers.Add(tilemapRenderer);

                        var gridCount = itemData.colliderGrids.Count / 4;
                        for (int j = 0; j < gridCount; j++)
                        {
                            int minX = itemData.colliderGrids[j * 4];
                            int minY = itemData.colliderGrids[j * 4 + 1];
                            int maxX = itemData.colliderGrids[j * 4 + 2];
                            int maxY = itemData.colliderGrids[j * 4 + 3];

                            List<Vector3Int> poses = new List<Vector3Int>();
                            List<TileBase> tileBases = new List<TileBase>();
                            for (int x = minX; x <= maxX; x++)
                            {
                                for (int y = minY; y <= maxY; y++)
                                {
                                    poses.Add(new Vector3Int(x, y));
                                    tileBases.Add(barrierTile);
                                }
                            }
                            tilemap.SetTiles(poses.ToArray(), tileBases.ToArray());
                        }
 
                         
                        itemInstance.transform.SetParent(itemParent, false);
                        var mapItemInstanceEditor = itemInstance.gameObject.AddComponent<MapItemInstanceEditor>();
                        mapItemInstanceEditor.InitData(itemData,item.blindHomeEquipment, item.instanceId, item.coordinate,item.eventReferenceDatas);
                    }
                }
            }
        }
    }

    public void CreatArea(int newArea ,BehaviorAreaType newAreatype)
    {
        GameObject areaObj = Instantiate(areaPrefab, areaParent);
        MapAreaEditor mapAreaEditor = areaObj.GetComponent<MapAreaEditor>();
        mapAreaEditor.SetData(newArea, newAreatype); 
        tilemapRenderers.Add(mapAreaEditor.tilemapRenderer);
    }
    private void InitMapArea(bool hideTilemap = false)
    {
        if (mapRoomData != null)
        {
            for(int i = 0; i < mapRoomData.npcBehaviorAreas.Count; i++)
            {
                var areaData = mapRoomData.npcBehaviorAreas[i];
                GameObject areaObj = Instantiate(areaPrefab, areaParent);
                MapAreaEditor mapAreaEditor = areaObj.GetComponent<MapAreaEditor>();
                mapAreaEditor.SetData(areaData);
               
            }
            for(int i = 0; i < mapRoomData.specialNpcBehaviorAreas.Count; i++)
            {
                var specialNpcBehaviorArea = mapRoomData.specialNpcBehaviorAreas[i];
                GameObject areaObj = Instantiate(areaPrefab, areaParent);
                MapAreaEditor mapAreaEditor = areaObj.GetComponent<MapAreaEditor>();
                mapAreaEditor.SetData(specialNpcBehaviorArea);
            }
        }
    }

    public GameObject Save(bool changeMapName = false)
    {
        List<int2> cells = new List<int2>();
        var boundary = tilemap.cellBounds;

        int2 minCoordinate = new int2(1000, 1000);
        int2 maxCoordinate = new int2(-1000, -1000);

        for (int x = boundary.xMin; x <= boundary.xMax; x++)
        {
            for (int y = boundary.yMin; y <= boundary.yMax; y++)
            {
                var tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                if (tile)
                {
                    minCoordinate.x = x < minCoordinate.x ? x : minCoordinate.x;
                    minCoordinate.y = y < minCoordinate.y ? y : minCoordinate.y;
                    maxCoordinate.x = x > maxCoordinate.x ? x : maxCoordinate.x;
                    maxCoordinate.y = y > maxCoordinate.y ? y : maxCoordinate.y;
                    
                    if(!(tile != null && tile.name == "1"))
                    {
                        cells.Add(new int2(x, y));
                    } 
                }
            }
        }
        mapRoomData.barrierGrids = GameCommon.CellToGrid(cells);
        mapRoomData.startCoordinate = minCoordinate;
        mapRoomData.endCoordinate = maxCoordinate;

        if (groundParent.childCount > 0)
        {
            string path = $"{prefabPath}{mapRoomData.roomName}{".prefab"}";
            if (changeMapName)
            {
                string oldPath = $"{prefabPath}{mapRoomData.name}{".prefab"}";
                AssetDatabase.DeleteAsset(oldPath);
            }
            return PrefabUtility.SaveAsPrefabAsset(groundParent.GetChild(0).gameObject, path);
        }
        return null;
    }

    public void SetMapName(string mapName)
    {
        gameObject.name = mapName;
        if (groundParent.childCount > 0)
        {
            groundParent.GetChild(0).name = mapName;
            mapRoomData.roomName = mapName;
        }
    }

    private void InitMapTile()
    {
        tilemap.ClearAllTiles();

        GameObject.DestroyImmediate(coordinateDisplayParent);
        coordinateDisplayParent = new GameObject("CoordinateDisplay");
        coordinateDisplayParent.transform.SetParent(transform, false);

        int gridCount = mapRoomData.barrierGrids.Count / 4;
        HashSet<Vector3Int> barriers = new HashSet<Vector3Int>();
        for (int i = 0; i < gridCount; i++)
        {
            int minX = mapRoomData.barrierGrids[i * 4];
            int minY = mapRoomData.barrierGrids[i * 4 + 1];
            int maxX = mapRoomData.barrierGrids[i * 4 + 2];
            int maxY = mapRoomData.barrierGrids[i * 4 + 3];
            
            List<Vector3Int> poses=new List<Vector3Int>();
            List<TileBase> tileBases = new List<TileBase>();
            for(int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    poses.Add(new Vector3Int(x, y));
                    tileBases.Add(barrierTile);
                    barriers.Add(new Vector3Int(x, y)); 
                    Vector2 pos = GameCommon.GetMapPos(x,y);
                    var editorCoordinate = Instantiate(this.editorCoordinate, pos, Quaternion.identity, coordinateDisplayParent.transform);
                    editorCoordinate.SetText($"{x},{y}");
                    coordinateDisplayParent.SetActive(displayCoordinate);
                }
            }
            tilemap.SetTiles(poses.ToArray(), tileBases.ToArray());
        }

        List<Vector3Int> poses1 = new List<Vector3Int>();
        List<TileBase> tileBases1 = new List<TileBase>();
        for (int x=mapRoomData.startCoordinate.x; x <= mapRoomData.endCoordinate.x; x++)
        {
            for (int y = mapRoomData.startCoordinate.y; y <= mapRoomData.endCoordinate.y; y++)
            {
                var pos = new Vector3Int(x, y);
                if (!barriers.Contains(pos))
                {
                    poses1.Add(pos);
                    tileBases1.Add(walkTile);
                }
            }
        }
        tilemap.SetTiles(poses1.ToArray(), tileBases1.ToArray());
    }

    private Vector3 oldPos;
    public bool UpDataPos;

    private void OnDestroy()
    {
        if (WorldInstanceEditor.Instance)
        {
            WorldInstanceEditor.Instance.DeleteMapInstance(this);
        }

    }
    public int2 coordinate;
    bool oldhideTilemap;
    bool oldDisplayCoordinate;
    private void Update()
    {
        if (oldDisplayCoordinate != displayCoordinate)
        {
            oldDisplayCoordinate = displayCoordinate;
            coordinateDisplayParent.SetActive(displayCoordinate);
        }

        if (oldhideTilemap != hideTilemap)
        {
            oldhideTilemap = hideTilemap;
            tilemapRenderers.RemoveAll(t => t == null);
            foreach(var renderer in tilemapRenderers)
            {
                renderer.enabled = !hideTilemap;
            }
        }

        if (UpDataPos)
        {
            if (oldPos != transform.position)
            {
                coordinate = GameCommon.GetMapCoordinateInt(transform.position);
                var truePos = GameCommon.GetZeroMapPos(coordinate);
                transform.position = new Vector3(truePos.x, truePos.y, transform.position.z);
                oldPos = transform.position;
            }
        }
    }
}
#endif
