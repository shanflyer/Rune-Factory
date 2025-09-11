#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class MapLinkEditor : MonoBehaviour
{
    private Transform startPoint, endPoint;
    private Vector3 startPos, endPos;
    private int2 startCoordinate, endCoordinate;
    private LineRenderer lineRenderer;

    private Vector3 mapPos0, mapPos1;
    [SerializeField]
    private Tilemap tilemap0, tilemap1;

    private  static TileBase linkTile
    {
        get
        {
            if (_linkTile == null)
            {
                _linkTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/TileMap/Tiles/Event/1.asset");
            }
            return _linkTile;
        }
    }
    private static TileBase _linkTile;
    private void OnEnable()
    {
        startPoint = transform.GetChild(0);
        endPoint = transform.GetChild(1);
        tilemap0=startPoint.GetChild(0).GetComponent<Tilemap>();
        tilemap1 = endPoint.GetChild(0).GetComponent<Tilemap>();
        startPos = startPoint.position;
        endPos = endPoint.position;
        lineRenderer = GetComponent<LineRenderer>();
        SetLineCoordinatePos();
    } 
    public MapLine mapLine;
    private MapInstanceEditor mapInstance0, mapInstance1;


    void SetLineCoordinatePos()
    {

        startCoordinate = GameCommon.GetMapCoordinateInt(startPos);
        endCoordinate = GameCommon.GetMapCoordinateInt(endPos);

        startPos = GameCommon.GetMapPos(startCoordinate);
        endPos = GameCommon.GetMapPos(endCoordinate);
        startPoint.transform.position = startPos;
        endPoint.transform.position = endPos;
        SetLinePoint();
    }

    public bool CheckLink(int mapId)
    {
        return mapLine.map0 == mapId || mapLine.map1
            == mapId;
    }
    public void InitLinkData(MapLine mapLine)
    { 
        if (mapLine.instanceId == 0)
        {
            var id = $"{mapLine.map0}{mapLine.map1}";
            mapLine.instanceId = int.Parse(id);
        }
        this.mapLine = mapLine;
        mapInstance0 = WorldInstanceEditor.Instance.mapInstanceEditors[mapLine.map0];
        mapInstance1 = WorldInstanceEditor.Instance.mapInstanceEditors[mapLine.map1];

        mapPos0 = mapInstance0.transform.position;
        mapPos1 = mapInstance1.transform.position;

        var linkName = $"{mapLine.map0}-{mapLine.map1}";
        startPoint.gameObject.name = $"{linkName}:{mapLine.map0}";
        endPoint.gameObject.name = $"{linkName}:{mapLine.map1}";
        startPoint.parent.name = linkName;

        SetLinePointPos();
    }
    void SetLinePoint()
    {
        Vector3[] positions = new Vector3[2]
        {
            startPos,endPos
        };
        lineRenderer.SetPositions(positions);
    }

    void SetLinePointPos()
    {
        startCoordinate = mapLine.center0+ mapInstance0.coordinate;
        Vector2 _startPos = mapInstance0.GetMapPos(startCoordinate);
        startPoint.position = new Vector3(_startPos.x, _startPos.y, startPoint.position.z);
        endCoordinate = mapLine.center1 + mapInstance1.coordinate;
        Vector2 _endPos = mapInstance1.GetMapPos(endCoordinate);
        endPoint.position = new Vector3(_endPos.x, _endPos.y, endPoint.position.z);

        mapPos0 = mapInstance0.transform.position;
        mapPos1 = mapInstance1.transform.position;

        startPos = startPoint.position;
        endPos = endPoint.position;

        SetLinePoint();
        tilemap0.ClearAllTiles();
        var gridCount = mapLine.cells0.girds.Count / 4;
        for (int j = 0; j < gridCount; j++)
        {
            int minX = mapLine.cells0.girds[j * 4];
            int minY = mapLine.cells0.girds[j * 4 + 1];
            int maxX = mapLine.cells0.girds[j * 4 + 2];
            int maxY = mapLine.cells0.girds[j * 4 + 3];

            List<Vector3Int> poses = new List<Vector3Int>();
            List<TileBase> tileBases = new List<TileBase>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    poses.Add(new Vector3Int(x - startCoordinate.x, y - startCoordinate.y));
                    tileBases.Add(linkTile);
                }
            }
            tilemap0.SetTiles(poses.ToArray(), tileBases.ToArray());
        }

        tilemap1.ClearAllTiles();
        var gridCount1 = mapLine.cells1.girds.Count / 4;
        for (int j = 0; j < gridCount1; j++)
        {
            int minX = mapLine.cells1.girds[j * 4];
            int minY = mapLine.cells1.girds[j * 4 + 1];
            int maxX = mapLine.cells1.girds[j * 4 + 2];
            int maxY = mapLine.cells1.girds[j * 4 + 3];

            List<Vector3Int> poses = new List<Vector3Int>();
            List<TileBase> tileBases = new List<TileBase>();
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    poses.Add(new Vector3Int(x - endCoordinate.x, y - endCoordinate.y));
                    tileBases.Add(linkTile);
                }
            }
            tilemap1.SetTiles(poses.ToArray(), tileBases.ToArray());
        }


        var linkName = $"{mapLine.map0}-{mapLine.map1}";
        startPoint.gameObject.name = $"{linkName}:{mapLine.map0}";
        endPoint.gameObject.name = $"{linkName}:{mapLine.map1}";
        startPoint.parent.name = linkName;
    }

    public void CheckPos()
    {
        /*
        if (mapPos0 != mapInstance0.transform.position || mapPos1 != mapInstance1.transform.position)
        {
            SetLinePointPos();
        }

        if (!WorldInstanceEditor.Instance.InitLinkMap(startCoordinate, endCoordinate, ref mapLine))
        {
            DestroyImmediate(gameObject);
        }
        */
        mapInstance0 = WorldInstanceEditor.Instance.mapInstanceEditors[mapLine.map0];
        mapInstance1 = WorldInstanceEditor.Instance.mapInstanceEditors[mapLine.map1
        ];
    }

    private void Update()
    {
       startCoordinate = GameCommon.GetMapCoordinateInt(startPoint.position);
       endCoordinate = GameCommon.GetMapCoordinateInt(endPoint.position);
        startPos = GameCommon.GetMapPos(startCoordinate);
        endPos = GameCommon.GetMapPos(endCoordinate);
        startPoint.transform.position = startPos;
        endPoint.transform.position = endPos;
        SetLinePoint();

    }
    public void SetLinkMapData()
    {
        var trueStartCoordinate = startCoordinate - mapInstance0.coordinate;
        var trueEndCoordinate=endCoordinate-mapInstance1.coordinate;
        if (!WorldInstanceEditor.Instance.InitLinkMap(startCoordinate, endCoordinate,tilemap0,tilemap1,
            mapInstance0.coordinate,mapInstance1.coordinate,
             ref mapLine))
        { 


            DestroyImmediate(gameObject);
        }

        if (mapLine.instanceId == 0)
        {
            var id = $"{mapLine.map0}{mapLine.map1}";
            mapLine.instanceId = int.Parse(id);
        }
       
    }
    // Update is called once per frame
    void Update1()
    {
        if (mapPos0 != mapInstance0.transform.position || mapPos1 != mapInstance1.transform.position)
        {
            SetLinePointPos();
        }
        else if (startPos != startPoint.position || endPos != endPoint.position)
        {
            int2 startCoordinate = GameCommon.GetMapCoordinateInt(startPoint.position);
            Vector2 _startPos = GameCommon.GetMapPos(startCoordinate);
            startPoint.position = new Vector3(_startPos.x, _startPos.y, startPoint.position.z);

            int2 endCoordinate = GameCommon.GetMapCoordinateInt(endPoint.position);
            Vector2 _endPos = GameCommon.GetMapPos(endCoordinate);
            endPoint.position = new Vector3(_endPos.x, _endPos.y, endPoint.position.z);

            startPos = startPoint.position;
            endPos = endPoint.position;
            SetLinePoint();

            if (!WorldInstanceEditor.Instance.InitLinkMap(startCoordinate, endCoordinate, tilemap0, tilemap1, 
            mapInstance0.coordinate, mapInstance1.coordinate, 
             ref mapLine))
            {
                DestroyImmediate(gameObject);
            }

            mapInstance0 = WorldInstanceEditor.Instance.mapInstanceEditors[mapLine.map0];
            mapInstance1 = WorldInstanceEditor.Instance.mapInstanceEditors[mapLine.map1
            ];
        }
    }
}
#endif

