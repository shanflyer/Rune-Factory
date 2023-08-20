#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using Unity.Mathematics;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class WorldInstanceEditor : MonoBehaviour
{
    public static WorldInstanceEditor Instance;

    private WorldMapData worldMapData;
    public Dictionary<int, MapInstanceEditor> mapInstanceEditors = new Dictionary<int, MapInstanceEditor>();

    private Dictionary<string, MapRoomData> mapRoomDatas;

    private Transform mapParent, linkParent;
    private GameObject linkPerfab
    {
        get
        {
            if (_linkPrefab == null)
            {
                _linkPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/MapLink.prefab");
            }
            return _linkPrefab;
        }
    }
    private GameObject _linkPrefab;



    public void InitData(WorldMapData worldMapData, Dictionary<string, MapRoomData> mapRoomDatas)
    {
        Instance = this;

        this.worldMapData = worldMapData;
        this.mapRoomDatas = mapRoomDatas;

        GameObject mapParentObj = new GameObject("mapParent");
        mapParent = mapParentObj.transform;
        mapParent.SetParent(transform);
        GameObject linkParentObj = new GameObject("linkParent");
        linkParent = linkParentObj.transform;
        linkParent.SetParent(transform);

        InitMapInstance();
        InitMapLink();
    }
    public Vector3 GetMapPos(int mapId, Vector2Int coordinate)
    {
        if (mapInstanceEditors.TryGetValue(mapId, out MapInstanceEditor mapInstanceEditor))
        {
            Vector3 pos = GameCommon.GetMapPos(coordinate);
            return mapInstanceEditor.transform.localPosition + pos;
        }

        return Vector3.zero;
    }


    public bool InitLinkMap(int2 coordinate0, int2 coordinate1, ref MapLine mapLine)
    {
        bool init0 = false;
        bool init1 = false;

        int map0 = -1;
        int map1 = -1;
        foreach (var mapInstance in mapInstanceEditors.Values)
        {
            if (!init0)
            {
                if (mapInstance.CheckPos(ref coordinate0))
                {
                    init0 = true;
                    map0 = mapInstance.id;
                }
            }
            if (!init1)
            {
                if (mapInstance.CheckPos(ref coordinate1))
                {
                    init1 = true;
                    map1 = mapInstance.id;
                }
            }

            if (init0 && init1)
            {
                break;
            }
        }
        if (init0 && init1)
        {
            mapLine.map0 = map0;
            mapLine.map1 = map1;
            mapLine.cell0 = coordinate0;
            mapLine.cell1 = coordinate1;

            return true;
        }

        return false;
    }
    public void AddNewLink()
    {
        if (mapInstanceEditors.Count > 0)
        {
            var keys = mapInstanceEditors.Keys.ToList();
            int id0 = keys[0];
            int id1 = keys[keys.Count - 1];
            MapLine mapLine = new MapLine
            {
                map0 = mapInstanceEditors[id0].id,
                map1 = mapInstanceEditors[id1].id
            };
            GameObject linkObj = Instantiate(linkPerfab, linkParent);
            MapLinkEditor mapLinkEditor = linkObj.GetComponent<MapLinkEditor>();
            mapLinkEditor.InitLinkData(mapLine);
        }
    }

    public void DeleteMapInstance(MapInstanceEditor mapInstanceEditor)
    {
        if (mapInstanceEditors.ContainsKey(mapInstanceEditor.id))
        {
            mapInstanceEditors.Remove(mapInstanceEditor.id);
        }
        var links = FindObjectsOfType<MapLinkEditor>();
        foreach (var link in links)
        {
            if (link.CheckLink(mapInstanceEditor.id))
            {
                DestroyImmediate(link.gameObject);
            }
        }
    }

    private void InitMapInstance()
    {
        mapInstanceEditors.Clear();
        foreach (var worldMap in worldMapData.worldMaps)
        {
            var mapData = mapRoomDatas[worldMap.map];

            AddMapInstance(mapData, worldMap.id, worldMap.coordinate);
        }
    }

    private void InitMapLink()
    {
        foreach (var mapLine in worldMapData.mapLines)
        {
            GameObject linkObj = Instantiate(linkPerfab, linkParent);
            MapLinkEditor mapLinkEditor = linkObj.GetComponent<MapLinkEditor>();
            mapLinkEditor.InitLinkData(mapLine);
        }
    }

    private void OnDrawGizmos()
    {

    }

    public void AddMapInstance(MapRoomData mapData)
    {
        int id = mapInstanceEditors.Count;
        id = id * 100 + Random.Range(0, 99);
        while (mapInstanceEditors.ContainsKey(id))
        {
            id = id * 100 + Random.Range(0, 99);
        }

        GameObject mapObj = new GameObject($"{mapData.roomName}{id}");
        mapObj.transform.SetParent(mapParent, false);

        MapInstanceEditor mapInstanceEditor = mapObj.AddComponent<MapInstanceEditor>();
        mapInstanceEditor.InitData(mapData, id, true);
        mapInstanceEditor.UpDataPos = true;
        mapInstanceEditors.Add(id, mapInstanceEditor);

        Vector2Int coordiante = Vector2Int.zero;
        var pos = GameCommon.GetZeroMapPos(coordiante);
        mapObj.transform.position = new Vector3(pos.x, pos.y, 0);
    }

    private void AddMapInstance(MapRoomData mapData, int id, int3 coordinate)
    {
        GameObject mapObj = new GameObject($"{mapData.roomName}{id}");
        mapObj.transform.SetParent(mapParent, false);

        MapInstanceEditor mapInstanceEditor = mapObj.AddComponent<MapInstanceEditor>();
        mapInstanceEditor.InitData(mapData, id, true);
        mapInstanceEditor.UpDataPos = true;
        mapInstanceEditors.Add(id, mapInstanceEditor);

        Vector2Int coordiante = new Vector2Int(coordinate.x, coordinate.y);
        var pos = GameCommon.GetZeroMapPos(coordiante);
        mapObj.transform.position = new Vector3(pos.x, pos.y, coordinate.z);
    }

    // Start is called before the first frame update
    private void Start()
    {
    }


}
#endif
