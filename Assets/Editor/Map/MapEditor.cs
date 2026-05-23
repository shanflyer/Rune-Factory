using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class MapEditor : MyEditor
{
    public static MapEditor Instance;
    private List<MapRoomData> mapRoomDatas;

    private MapRoomDataObj selectMapRoomDataObj
    {
        get
        {
            return _selectMapRoomDataObj;
        }
        set
        {
            if (_selectMapRoomDataObj != value)
            {
                if (mapInstance)
                {
                    DestroyImmediate(mapInstance.gameObject);
                }

                _selectMapRoomDataObj = value;
                if (mapInstance)
                {
                    DestroyImmediate(mapInstance);
                }
                if (_selectMapRoomDataObj != null)
                {
                    GameObject mapInstanceObj = new GameObject(_selectMapRoomDataObj.GetName());
                    mapInstance = mapInstanceObj.AddComponent<MapInstanceEditor>();
                    mapInstance.InitData(_selectMapRoomDataObj.mapRoomData, 0);
                }
            }
        }
    }

    private MapRoomDataObj _selectMapRoomDataObj;
    private List<CommonObj> mapRoomDataObjs = new List<CommonObj>();

    private CommonEditor roomDataPanel;
    private MapInstanceEditor mapInstance;

    private Tilemap ground, collider, trigger;
    private TileBase colliderTile, triggerTile;

    [MenuItem("工具/地图编辑")]
    public static void WindowShow()
    {
        Instance = CreateWindow<MapEditor>("地图编辑");
        Instance.minSize = new Vector2(360, 480);
        Instance.maxSize = new Vector2(360, 480);
        Instance.ShowAuxWindow();
    }

    public new void ShowAuxWindow()
    {
        roomDataPanel = CreateInstance<CommonEditor>();
        roomDataPanel.InitData(Instance, null);

        MapItemEditor.LoadItemData();

        mapRoomDataObjs = new List<CommonObj>();
        mapRoomDatas = new List<MapRoomData>();
        var dir = new DirectoryInfo(EditorDataPath.mapRoomDataPath);
        var files = dir.GetFiles("*.asset");
        foreach (var file in files)
        {
            MapRoomData mapRoomData = AssetDatabase.LoadAssetAtPath<MapRoomData>($"{EditorDataPath.mapRoomDataPath}{file.Name}");
            mapRoomDatas.Add(mapRoomData);

            MapRoomDataObj mapRoomDataObj = new MapRoomDataObj(mapRoomData);
            mapRoomDataObjs.Add(mapRoomDataObj);
        }
        var MapEditor = GameObject.Find("MapEditor");
        if (MapEditor == null)
        {
            Debug.LogError("场景不对或无MapEditor物体！");
            return;
        }

        ground = MapEditor.transform.Find("Ground").GetComponent<Tilemap>();
        collider = MapEditor.transform.Find("Collider").GetComponent<Tilemap>();
        trigger = MapEditor.transform.Find("Trigger").GetComponent<Tilemap>();

        colliderTile = AssetDatabase.LoadAssetAtPath<TileBase>(EditorDataPath.colliderTile);
        triggerTile = AssetDatabase.LoadAssetAtPath<TileBase>(EditorDataPath.triggerTile);
    }

    private void DrawRoomDataPanel()
    {
        roomDataPanel.DisplayCommonObjList<MapRoomDataObj>(360, 380, mapRoomDataObjs, 3, false, false, true, false, true);
        if (selectMapRoomDataObj != null)
        {
            DrawTextField(selectMapRoomDataObj.mapRoomData.roomName, "地图名字", SetMapRoomName, 60, 100);
        }
    }

    private void SetMapRoomName(string mapName)
    {
        changeMapName = true;
        selectMapRoomDataObj.mapRoomData.roomName = mapName;
        if (mapInstance)
        {
            mapInstance.SetMapName(mapName);
        }
    }

    private bool changeMapName = false;

    public new void OnDestroy()
    {
        base.OnDestroy();
        if (mapInstance)
        {
            DestroyImmediate(mapInstance.gameObject);
        }
    }

    int newArea = 999;
    BehaviorAreaType newAreatype = BehaviorAreaType.创建;
    public void OnGUI()
    {
        DrawRoomDataPanel();
        EditorGUILayout.BeginHorizontal();
        DrawIntField(ref newArea, "区域名字", 60);
        DrawEnum(newAreatype, "区域类型", (Enum value) => { newAreatype = (BehaviorAreaType)value; });
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("新建区域"))
        {
            mapInstance.CreatArea(newArea,newAreatype);
        }
    }

    private string AutoNewRoomName()
    {
        int count = mapRoomDataObjs.Count;
        string roomName = $"{"NewRoom"}{count}";
        while (mapRoomDatas.Exists(m => m.roomName == roomName))
        {
            count++;
            roomName = $"{"NewRoom"}{count}";
        }
        return roomName;
    }

    public void SelectMapRoom(MapRoomDataObj mapRoomDataObj)
    {
        selectMapRoomDataObj = mapRoomDataObj;
    }

    public CommonObj CreatNewRoom()
    {
        changeMapName = false;
        MapRoomData roomData = new MapRoomData
        {
            roomName = AutoNewRoomName(),
        };
        mapRoomDatas.Add(roomData);

        selectMapRoomDataObj = new MapRoomDataObj(roomData);
        mapRoomDataObjs.Add(selectMapRoomDataObj);

        return selectMapRoomDataObj;
    }

    public void DeleteSelectRoom()
    {
        if (selectMapRoomDataObj != null)
        {
            mapRoomDataObjs.Remove(selectMapRoomDataObj);
            mapRoomDatas.Remove(selectMapRoomDataObj.mapRoomData);
            selectMapRoomDataObj = null;
        }
    }

    public int CreatMapItemInstance(int id)
    {
        // 生成实例 ID 只需要检查已有对象，包含未激活对象但不依赖排序。
        var mapItemInstances = FindObjectsByType<MapItemInstanceEditor>(FindObjectsInactive.Include).ToList();
        int intanceid = id * 1000 + Random.Range(0, 1000);
        while (mapItemInstances.Exists(m => m.mapItem.instanceId == intanceid))
        {
            intanceid = id * 1000 + Random.Range(0, 1000);
        }
        return intanceid;
    }

    public void SaveSelectMap()
    {
        if (selectMapRoomDataObj != null && mapInstance != null)
        {
            GameObject roomObj = mapInstance.Save(changeMapName);
            //selectMapRoomDataObj.mapRoomData.mapObj = roomObj;

            selectMapRoomDataObj.mapRoomData.mapItems.Clear();
            var mapItemInstances = FindObjectsByType<MapItemInstanceEditor>();
            HashSet<int> instanceId = new HashSet<int>();
            foreach (var mapItemInstance in mapItemInstances)
            {
                if (instanceId.Contains(mapItemInstance.mapItem.instanceId))
                {
                    mapItemInstance.mapItem.instanceId = Instance.CreatMapItemInstance(mapItemInstance.mapItem.id);
                }
                instanceId.Add(mapItemInstance.mapItem.instanceId);
                selectMapRoomDataObj.mapRoomData.mapItems.Add(mapItemInstance.mapItem);
            }

            var mapAreas = FindObjectsByType<MapAreaEditor>();
            selectMapRoomDataObj.mapRoomData.npcBehaviorAreas.Clear();
            selectMapRoomDataObj.mapRoomData.specialNpcBehaviorAreas.Clear();
            foreach (var mapArea in mapAreas)
            {
                var data = mapArea.GetAreaData();
                if (data != null)
                {
                    selectMapRoomDataObj.mapRoomData.npcBehaviorAreas.Add(data);
                }
                else
                {
                    var specialData=mapArea.GetSpecialData();
                    if (specialData != null)
                    {
                        selectMapRoomDataObj.mapRoomData.specialNpcBehaviorAreas.Add(specialData);
                    }
                }
            }

            if (AssetDatabase.Contains(selectMapRoomDataObj.mapRoomData))
            {
                EditorUtility.SetDirty(selectMapRoomDataObj.mapRoomData);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                AssetDatabase.MoveAsset($"{EditorDataPath.mapRoomDataPath}{selectMapRoomDataObj.mapRoomData.name}{".asset"}", $"{EditorDataPath.mapRoomDataPath}{selectMapRoomDataObj.GetName()}{".asset"}");
                //selectMapRoomDataObj.mapRoomData.name = selectMapRoomDataObj.GetName();
                EditorUtility.SetDirty(selectMapRoomDataObj.mapRoomData);

                /*
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();*/
            }
            else
            {
                AssetDatabase.CreateAsset(selectMapRoomDataObj.mapRoomData, $"{EditorDataPath.mapRoomDataPath}{selectMapRoomDataObj.GetName()}{".asset"}");
            }
        }
    }
}
