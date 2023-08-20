using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public class MapItemEditor : MyEditor
{
    public static MapItemEditor Instance;
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

    public new void ShowAuxWindow()
    {
        mapItemDataObjs.Clear();
        DirectoryInfo mapItemDir = new DirectoryInfo(EditorDataPath.mapItemDataPath);
        var files = mapItemDir.GetFiles("*.asset");
        MapInstanceEditor.mapItemDatas = new Dictionary<int, MapItemData>();
        foreach (var file in files)
        {
            var mapItemData= AssetDatabase.LoadAssetAtPath<MapItemData>($"{EditorDataPath.mapItemDataPath}{file.Name}");
            mapItemDataObjs.Add(new MapItemDataObj(mapItemData));
            MapInstanceEditor.mapItemDatas.Add(mapItemData.id, mapItemData);
        }
         
        mapItemsPanel = CreateInstance<CommonEditor>();
        mapItemsPanel.InitData(Instance, null);
    }

    private Transform itemParent
    {
        get
        {
            if (_itemParent == null)
            {
                _itemParent = FindObjectOfType<MapInstanceEditor>().transform.GetChild(1);
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
                var mapItemInstanceEditor = itemObj.AddComponent<MapItemInstanceEditor>();
                mapItemInstanceEditor.InitData(selectMapItemDataObj.itemData,MapEditor.Instance.CreatMapItemInstance(selectMapItemDataObj.itemData.id), Vector2Int.zero);
            }
        }
    }

    public void OnGUI()
    {
        mapItemsPanel.DisplayCommonObjList<MapItemDataObj>(200, 240, mapItemDataObjs, 2, false);
        DrawButton("创建地图道具:", CreatMapItem, 200);
    }
}