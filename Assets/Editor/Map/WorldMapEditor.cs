using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using System.Collections.Generic;
using System.IO; 
using UnityEditor;
using UnityEngine;

public class WorldMapEditor : MyEditor
{
    public static WorldMapEditor Instance; 
    private CommonEditor roomMapPanel;
    private CommonEditor worldPanel;
    

    private List<CommonObj> mapRoomObjs = new List<CommonObj>();
    private Dictionary<string, MapRoomData> mapRoomDatas = new Dictionary<string, MapRoomData>();

    private List<CommonObj> worldObjs = new List<CommonObj>();
    private WorldDataObj selectWorld;
    private WorldInstanceEditor worldInstanceEditor;

    [MenuItem("工具/世界地图")]
    public static void WindowShow()
    {
        WorldMapEditor worldMapEditor = CreateWindow<WorldMapEditor>("世界编辑");
        Instance = worldMapEditor;
        //worldMapEditor.minSize = worldMapEditor.maxSize = new Vector2(240, 480);
        Instance.ShowAuxWindow();
    }

    public new void ShowAuxWindow()
    {
        roomMapPanel = CreateInstance<CommonEditor>();
        roomMapPanel.InitData(Instance, null);
        MapItemEditor.LoadItemData();
        worldPanel =CreateInstance<CommonEditor>();
        worldPanel.InitData(Instance, null);
        LoadWorldData();
        LoadRoomData();  
    }

    public CommonObj CreatWorld()
    {
        WorldMapData worldMapData = new WorldMapData
        {
            name = $"新世界{worldObjs.Count}",
        };
        WorldDataObj worldDataObj = new WorldDataObj(worldMapData);
        worldObjs.Add(worldDataObj);
        SelectWorld(worldDataObj);
        return worldDataObj;
    }
    public void DeleteWorld(WorldDataObj worldDataObj)
    {
        string path=$"{EditorDataPath.worldMapDataPath}{worldDataObj.GetName()}.asset";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        worldObjs.Remove(worldDataObj);
        if (selectWorld == worldDataObj)
        {
            selectWorld = null;
        }
        if (worldInstanceEditor != null)
        {
            DestroyImmediate(worldInstanceEditor.gameObject);
        }
    }
    public void SelectWorld(WorldDataObj worldDataObj)
    {
        selectWorld = worldDataObj;
        if (selectWorld == null)
        {
            return;
        }
        CreatWorldInstance();
    }
   
    private void LoadWorldData()
    {
        DirectoryInfo worldDir = new DirectoryInfo(EditorDataPath.worldMapDataPath);
        var files = worldDir.GetFiles("*.asset");
        worldObjs.Clear();
        foreach(var file in files)
        {
            var worldData=AssetDatabase.LoadAssetAtPath<WorldMapData>($"{EditorDataPath.worldMapDataPath}{file.Name}");
            worldObjs.Add(new WorldDataObj(worldData));
        }
        if (worldObjs.Count > 0)
        {
            //selectWorld = worldObjs[0];
        }
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
        if (worldInstanceEditor != null)
        {
            DestroyImmediate(worldInstanceEditor.gameObject);
        }
        GameObject worldObj = new GameObject("World");
        worldInstanceEditor = worldObj.AddComponent<WorldInstanceEditor>();
        worldInstanceEditor.InitData(selectWorld.data, mapRoomDatas);
    }

    public void OnGUI()
    {
        roomMapPanel.DisplayCommonObjList<MapRoomDataObj>(240, (int)(Instance.position.size.y*0.45f), mapRoomObjs, 2);

        EditorGUILayout.BeginVertical("button");
        worldPanel.DisplayCommonObjList<WorldDataObj>(240, (int)(Instance.position.size.y * 0.3f), worldObjs, 2,false,false,true);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();
        DrawButton("添加地图", CreatNewMapInstance, 80);
        DrawButton("添加链接", CreatMapLink, 80);
        DrawButton("保存", SaveWorldData, 80);
        EditorGUILayout.EndHorizontal();
       
    }

    void SaveWorldData()
    {
        if(selectWorld == null)
        {
            return;
        }

        var mapInstances = FindObjectsByType<MapInstanceEditor>(FindObjectsSortMode.None);
        var links = FindObjectsByType<MapLinkEditor>(FindObjectsSortMode.None);

        

        selectWorld.data.mapLines.Clear();
        selectWorld.data.worldMaps.Clear();

        foreach(var mapInstance in mapInstances)
        {
            selectWorld.data.worldMaps.Add(mapInstance.GetWorldMap());
        }
        foreach(var mapLink in links)
        {
            mapLink.SetLinkMapData();
            if (mapLink != null)
            {
                selectWorld.data.mapLines.Add(mapLink.mapLine);
            }
            
        }
        if (AssetDatabase.Contains(selectWorld.data))
        {
            EditorUtility.SetDirty(selectWorld.data);
            AssetDatabase.SaveAssets();
        }
        else
        {
            string path=$"{EditorDataPath.worldMapDataPath}{selectWorld.GetName()}.asset";
            AssetDatabase.CreateAsset(selectWorld.data,path); 
        } 
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