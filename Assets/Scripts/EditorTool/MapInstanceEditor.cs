#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class MapInstanceEditor : MonoBehaviour
{

    private MapRoomData mapRoomData;
    private Transform groundParent;
    private Transform itemParent;

    private Tilemap tilemap;

    public static Dictionary<int, MapItemData> mapItemDatas;

    private static string defaultGroundPath = "Assets/Resources/Prefabs/DefaultGround.prefab";

    private static string prefabPath = "Assets/Resources/Prefabs/Ground/";

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

    private static TileBase walkTile
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

    private static TileBase barrierTile
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

    private static TileBase _walkTile;
    private static TileBase _barrierTile;
    public int id;

    public WorldMap GetWorldMap()
    {
        WorldMap worldMap = new WorldMap
        {
            map = mapRoomData.name,
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

        if (groundParent == null)
        {
            var groundParentObj = transform.Find("GroundParent");
            if (groundParentObj == null)
            {
                groundParentObj = new GameObject("GroundParent").transform;
                groundParentObj.transform.SetParent(transform, false); 
                groundParent = groundParentObj.transform;
                groundParent.localPosition = new Vector3(GameCommon.cellSize, GameCommon.cellSize);

                GameObject itemParentObj = new GameObject("ItemParent");
                itemParentObj.transform.SetParent(transform, false);
                itemParentObj.transform.localPosition = new Vector3(0, 0, -100);
                itemParent = itemParentObj.transform;

                GameObject Grid = new GameObject("Grid");
                Grid.transform.SetParent(transform, false);

                GameObject MapTile = new GameObject("MapTile");
                MapTile.transform.SetParent(Grid.transform, false);

                var grid = Grid.AddComponent<Grid>();
                tilemap = MapTile.AddComponent<Tilemap>();
                var tilemapRenderer = MapTile.AddComponent<TilemapRenderer>();

                tilemapRenderer.sortingOrder = 1;
                grid.cellSize = new Vector3(GameCommon.cellWidth, GameCommon.cellHigh, 0);
                tilemapRenderer.enabled = !hideTilemap;
            }
            InitMapObj();
            InitMapTile();
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
    private void InitMapObj()
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
                        itemObj.transform.SetParent(itemParent, false);
                        var mapItemInstanceEditor = itemObj.AddComponent<MapItemInstanceEditor>();
                        mapItemInstanceEditor.InitData(itemData, item.instanceId, item.coordinate);
                    }
                }
            }
        }
    }

    public GameObject Save(bool changeMapName = false)
    {
        mapRoomData.mapCells.Clear();
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

                    MapCellData mapCell = new MapCellData
                    {
                        coordinate = new int2(x, y),
                        isWalkable = (tile != null && tile.name == "1") ? true : false
                    };
                    mapRoomData.mapCells.Add(mapCell);
                }
            }
        }
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
        foreach (var tileData in mapRoomData.mapCells)
        {
            TileBase tileBase = tileData.isWalkable ? walkTile : barrierTile;
            tilemap.SetTile(new Vector3Int(tileData.coordinate.x, tileData.coordinate.y, 0),
                tileBase);
        }
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
    int2 coordinate;
    private void Update()
    {
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
