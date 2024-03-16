using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using static MapCellController;

public class MapItemEditor : MyEditor
{
    public static MapItemEditor Instance
    {
        get
        {
            if (_Instance == null)
            {
                WindowShow();
            }
            return _Instance;
        }
        set
        {
            _Instance = value;
        }
    }

    private static MapItemEditor _Instance;
    private CommonEditor mapItemsPanel;

   // private MapItemDataList mapItemDataList;

    private List<CommonObj> mapItemDataObjs = new List<CommonObj>();

    private MapItemDataObj selectMapItemDataObj
    {
        get
        {
            return _selectMapItemDataObj;
        }
        set
        {
            if (_selectMapItemDataObj != value)
            {
                _selectMapItemDataObj = value;
            }
        }
    }

    private MapItemDataObj _selectMapItemDataObj;
    private Tilemap ground, collider, trigger;
    private Transform singleItemParent;
    private MyInstance myInstance;
    private TileBase colliderTile, triggerTile,playerTriggerTile;
    private void OnDestroy()
    {
        base.OnDestroy();
        ground.ClearAllTiles();
        collider.ClearAllTiles();
        trigger.ClearAllTiles();
        ground.RefreshAllTiles();
        collider.RefreshAllTiles();
        ground.RefreshAllTiles();
        foreach (Transform child in singleItemParent)
        {
            DestroyImmediate(child.gameObject);
        }
        myInstance = null;
        _Instance = null;
    }
    [MenuItem("工具/地图道具")]
    public static void WindowShow()
    {
        _Instance = EditorWindow.CreateWindow<MapItemEditor>("地图道具"); 
        Instance.Init();
    }
    public new void ShowAuxWindow()
    {
        Init();
    }

    public static void LoadItemData()
    {
        DirectoryInfo mapItemDir = new DirectoryInfo(EditorDataPath.mapItemDataPath);
        var files = mapItemDir.GetFiles("*.asset");
        MapInstanceEditor.mapItemDatas = new Dictionary<int, MapItemData>();
        foreach (var file in files)
        {
            var mapItemData = AssetDatabase.LoadAssetAtPath<MapItemData>($"{EditorDataPath.mapItemDataPath}{file.Name}");
            
            MapInstanceEditor.mapItemDatas.Add(mapItemData.id, mapItemData);
        }
    }
    void Init()
    {
        mapItemDataObjs.Clear();
        LoadItemData();
        foreach(var data in MapInstanceEditor.mapItemDatas)
        {
            mapItemDataObjs.Add(new MapItemDataObj(data.Value));
        }
        
        mapItemsPanel = CreateInstance<CommonEditor>();
        mapItemsPanel.InitData(Instance, null);
        myInstance = new MyInstance();

        var MapEditor = GameObject.Find("MapEditor");
        if (MapEditor == null)
        {
            Debug.LogError("场景不对或无MapEditor物体！");
            return;
        }
        ground = MapEditor.transform.Find("Ground").GetComponent<Tilemap>();
        collider = MapEditor.transform.Find("Collider").GetComponent<Tilemap>();
        trigger = MapEditor.transform.Find("Trigger").GetComponent<Tilemap>();
        singleItemParent = GameObject.Find("ItemParent").transform;

        colliderTile = AssetDatabase.LoadAssetAtPath<TileBase>(EditorDataPath.colliderTile);
        triggerTile = AssetDatabase.LoadAssetAtPath<TileBase>(EditorDataPath.triggerTile);
        playerTriggerTile = AssetDatabase.LoadAssetAtPath<TileBase>(EditorDataPath.playerTriggerTile);
    }

    private Transform itemParent
    {
        get
        {
            if (_itemParent == null)
            {
                _itemParent = FindAnyObjectByType<MapInstanceEditor>().transform.GetChild(1);
            }
            return _itemParent;
        }
    }

    private Transform _itemParent;

    public void SelectMapItem(MapItemDataObj mapItemDataObj)
    {
        selectMapItemDataObj = mapItemDataObj;
    }

    private void CreatMapItem()
    {
        if (selectMapItemDataObj != null)
        {
            if (selectMapItemDataObj.itemData.itemObj)
            {
                GameObject itemObj = (GameObject)PrefabUtility.InstantiatePrefab(selectMapItemDataObj.itemData.itemObj);
                itemObj.transform.SetParent(itemParent, false);
                itemObj.transform.localPosition = Vector3.zero;
                var mapItemInstanceEditor = itemObj.AddComponent<MapItemInstanceEditor>();
                mapItemInstanceEditor.InitData(selectMapItemDataObj.itemData,MapEditor.Instance.CreatMapItemInstance(selectMapItemDataObj.itemData.id), Vector2Int.zero);
            }
        }
    }
    [SerializeField]
    GameObject selectItem; 
    private void NewMapItem()
    {
        DestroyImmediate(singleItemParent.gameObject);
        GameObject newItemParent = new GameObject("ItemParent");
        singleItemParent = newItemParent.transform;
        //newItemParent.transform.localPosition = new Vector3(GameCommon.cellSize, GameCommon.cellSize, 0);
        ground.enabled = false;
        collider.ClearAllTiles();
        trigger.ClearAllTiles();
        collider.RefreshAllTiles();
        trigger.RefreshAllTiles();

        selectItem = new GameObject("NewMapObj");
        selectItem.transform.SetParent(singleItemParent, false);
        selectItem.transform.localPosition = new Vector3(GameCommon.cellSize, GameCommon.cellSize, 0);

        GameObject Model = new GameObject("Model");
        GameObject Show = new GameObject("Show");
        Model.transform.SetParent(selectItem.transform, false);
        Show.transform.SetParent(selectItem.transform, false);


        MapItemData mapItemData = ScriptableObject.CreateInstance<MapItemData>();

        mapItemData.id = myInstance.CreatInstanceId();
        mapItemData.name = mapItemData.id.ToString();
        mapItemData.itemName = "NewMapObj";
        mapItemData.itemObj = selectItem;

        selectMapItemDataObj = new MapItemDataObj(mapItemData);


    }
    private void EditMapItem()
    {
        if (selectMapItemDataObj == null)
        {
            return;
        }
        DestroyImmediate(singleItemParent.gameObject);
        GameObject newItemParent = new GameObject("ItemParent");
        singleItemParent = newItemParent.transform;
        //newItemParent.transform.localPosition = new Vector3(GameCommon.cellSize, GameCommon.cellSize, 0);

        ground.enabled = false;
        collider.ClearAllTiles();
        trigger.ClearAllTiles();
        ground.ClearAllTiles();
        ground.RefreshAllTiles();
        collider.RefreshAllTiles();
        trigger.RefreshAllTiles();

        for(int i = 0; i < selectMapItemDataObj.itemData.colliderCells.Length; i++)
        {
            var coordinate = selectMapItemDataObj.itemData.colliderCells[i];
            collider.SetTile(new Vector3Int(coordinate.x, coordinate.y, 0), colliderTile);
        }
        for (int i = 0; i < selectMapItemDataObj.itemData.triggerCells.Length; i++)
        {
            var coordinate = selectMapItemDataObj.itemData.triggerCells[i];
            trigger.SetTile(new Vector3Int(coordinate.x, coordinate.y, 0), triggerTile);
        } 

        if (selectMapItemDataObj.itemData.playerTriggerCells != null)
        {
            for (int i = 0; i < selectMapItemDataObj.itemData.playerTriggerCells.Length; i++)
            {
                var coordinate = selectMapItemDataObj.itemData.playerTriggerCells[i];
                ground.SetTile(new Vector3Int(coordinate.x, coordinate.y, 0), playerTriggerTile);
            }
        }
       

        selectItem = Instantiate(selectMapItemDataObj.itemData.itemObj);
        selectItem.transform.SetParent(singleItemParent, false);
        selectItem.transform.localPosition = new Vector3(GameCommon.cellSize, GameCommon.cellSize, 0);
    }
    private void SaveMapItem()
    {
        var colliderBound = collider.cellBounds;
        List<int2> colliderCells = new List<int2>();
        for (int x = colliderBound.xMin; x < colliderBound.xMax; x++)
        {
            for (int y = colliderBound.yMin; y < colliderBound.yMax; y++)
            {
                var tile = collider.GetTile(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    colliderCells.Add(new int2(x, y));
                }
            }
        }
        selectMapItemDataObj.itemData.colliderCells = colliderCells.ToArray();

        var triggerBound = trigger.cellBounds;
        List<int2> triggerCells = new List<int2>();
        for (int x = triggerBound.xMin; x < triggerBound.xMax; x++)
        {
            for (int y = triggerBound.yMin; y < triggerBound.yMax; y++)
            {
                var tile = trigger.GetTile(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    triggerCells.Add(new int2(x, y));
                }
            }
        }
        selectMapItemDataObj.itemData.triggerCells = triggerCells.ToArray();

        var playerTriggerBound = ground.cellBounds;
        List<int2> playerTriggerCells = new List<int2>();
        for (int x = playerTriggerBound.xMin; x < playerTriggerBound.xMax; x++)
        {
            for (int y = playerTriggerBound.yMin; y < playerTriggerBound.yMax; y++)
            {
                var tile = ground.GetTile(new Vector3Int(x, y, 0));
                if (tile != null)
                {
                    playerTriggerCells.Add(new int2(x, y));
                }
            }
        }
        selectMapItemDataObj.itemData.playerTriggerCells = playerTriggerCells.ToArray();

        string assetPath = $"{EditorDataPath.mapItemDataPath}{selectMapItemDataObj.GetId()}.asset";
        string objPath = $"{EditorDataPath.mapItemPrefabPath}{selectMapItemDataObj.GetName()}.prefab";
        PrefabUtility.SaveAsPrefabAsset(selectItem, objPath);
        AssetDatabase.Refresh();
        AssetDatabase.ImportAsset(objPath);

        var obj = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
        selectMapItemDataObj.itemData.itemObj = obj;
        if (!mapItemDataObjs.Contains(selectMapItemDataObj))
        {
            mapItemDataObjs.Add(selectMapItemDataObj); 
        }
        EditorUtility.SetDirty(selectMapItemDataObj.itemData);
        if (AssetDatabase.Contains(selectMapItemDataObj.itemData))
        {
            AssetDatabase.SaveAssets();
        }
        else
        {
            AssetDatabase.CreateAsset(selectMapItemDataObj.itemData, assetPath);
        }

    }
    public void OnGUI()
    {
        if (mapItemsPanel == null)
        {
            Init();
        }

        mapItemsPanel.DisplayCommonObjList<MapItemDataObj>(300, 360, mapItemDataObjs, 3, false);

        EditorGUILayout.BeginHorizontal();
        DrawButton("新建地图物体", NewMapItem, 100);
        DrawButton("编辑地图物体", EditMapItem, 100);
        DrawButton("保存", SaveMapItem, 50);
        EditorGUILayout.EndHorizontal();
        DrawButton("创建地图道具", CreatMapItem, 100);

        DisplayMapObjProperty();
    }
    void DisplayMapObjProperty()
    {
        GUILayout.BeginVertical("button");
        if(selectMapItemDataObj!=null)
        {
            DrawIntField(ref selectMapItemDataObj.itemData.id, "物体Id:", 80, 120);
            DrawTextField(ref selectMapItemDataObj.itemData.itemName, "物体名字:", 80, 120);

        }
        GUILayout.EndVertical();
    }
}