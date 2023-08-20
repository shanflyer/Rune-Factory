using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class WorldMapEditor : MyEditor
{
    public static WorldMapEditor Instance;
    private WorldMapData worldMapData;
    private CommonEditor roomMapPanel;
    private WorldInstanceEditor worldInstanceEditor;

    private List<CommonObj> mapRoomObjs = new List<CommonObj>();
    private Dictionary<string, MapRoomData> mapRoomDatas = new Dictionary<string, MapRoomData>();

    [MenuItem("工具/世界地图")]
    public static void WindowShow()
    {
        WorldMapEditor worldMapEditor = CreateWindow<WorldMapEditor>("世界编辑");
        Instance = worldMapEditor;
        worldMapEditor.minSize = worldMapEditor.maxSize = new Vector2(240, 480);
        Instance.ShowAuxWindow();
    }

    public new void ShowAuxWindow()
    {
        roomMapPanel = CreateInstance<CommonEditor>();
        roomMapPanel.InitData(Instance, null);

        LoadWorldData();
        LoadRoomData();
         
        MapInstanceEditor.mapItemDatas = new Dictionary<int, MapItemData>();
        string itemDataPath = "Assets/Resources/Data/MapItemData/";
        DirectoryInfo mapItemDir = new DirectoryInfo(itemDataPath);
        var files = mapItemDir.GetFiles("*.asset");
        foreach (var file in files)
        {
            var mapItemData = AssetDatabase.LoadAssetAtPath<MapItemData>($"{itemDataPath}{file.Name}"); 
            MapInstanceEditor.mapItemDatas.Add(mapItemData.id, mapItemData);
        }
        
        CreatWorldInstance();
    }

    private void LoadWorldData()
    {
        worldMapData = AssetDatabase.LoadAssetAtPath<WorldMapData>(EditorDataPath.worldMapDataPath);
    }

    private void LoadRoomData()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(EditorDataPath.mapRoomDataPath);

        mapRoomObjs.Clear();
        mapRoomDatas.Clear();
        var files = directoryInfo.GetFiles("*.asset");
        foreach (var file in files)
        {
            string dataPath = $"{EditorDataPath.mapRoomDataPath}{file.Name}";
            MapRoomData mapRoomData = AssetDatabase.LoadAssetAtPath<MapRoomData>(dataPath);
            MapRoomDataObj mapRoomDataObj = new MapRoomDataObj(mapRoomData);
            mapRoomObjs.Add(mapRoomDataObj);

            mapRoomDatas.Add(mapRoomData.name, mapRoomData);
        }
    }

    private void CreatWorldInstance()
    {
        GameObject worldObj = new GameObject("World");
        worldInstanceEditor = worldObj.AddComponent<WorldInstanceEditor>();
        worldInstanceEditor.InitData(worldMapData, mapRoomDatas);
    }

    public void OnGUI()
    {
        roomMapPanel.DisplayCommonObjList<MapRoomDataObj>(220, 400, mapRoomObjs, 2);

        EditorGUILayout.BeginHorizontal();
        DrawButton("添加地图", CreatNewMapInstance, 80);
        DrawButton("添加链接", CreatMapLink, 80);
        DrawButton("保存", SaveWorldData, 80);
        EditorGUILayout.EndHorizontal();
       
    }

    void SaveWorldData()
    {
        var mapInstances = FindObjectsOfType<MapInstanceEditor>();
        var links = FindObjectsOfType<MapLinkEditor>();

        worldMapData.mapLines.Clear();
        worldMapData.worldMaps.Clear();

        foreach(var mapInstance in mapInstances)
        {
            worldMapData.worldMaps.Add(mapInstance.GetWorldMap());
        }
        foreach(var mapLink in links)
        {
            worldMapData.mapLines.Add(mapLink.mapLine);
        }

        EditorUtility.SetDirty(worldMapData);
        AssetDatabase.SaveAssets();
    }
    void CreatMapLink()
    {
        if (worldInstanceEditor)
        {
            worldInstanceEditor.AddNewLink();
        }
    }

     void CreatNewMapInstance()
    {
        if (selectRoomDataObj != null)
        {
            worldInstanceEditor.AddMapInstance(selectRoomDataObj.mapRoomData);
        }
       
    }

    public new void OnDestroy()
    {
        if (worldInstanceEditor != null)
        {
            DestroyImmediate(worldInstanceEditor.gameObject);
        }
    }

    private MapRoomDataObj selectRoomDataObj;

    public void SelectMapRoom(MapRoomDataObj mapRoomDataObj)
    {
        selectRoomDataObj = mapRoomDataObj;
    }
}