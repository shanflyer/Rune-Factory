using Codice.CM.Client.Differences;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class CommonToolEditor:MyEditor
{
    public static CommonToolEditor Instance;
    [MenuItem("工具/通用工具")]
    public static void WindowShow()
    {
        Instance = EditorWindow.CreateWindow<CommonToolEditor>("通用工具");
        Instance.minSize = new Vector2(240, 360);
        Instance.maxSize = new Vector2(240, 360);
        Instance.ShowAuxWindow();
    }
    private void OnGUI()
    {
        if (GUILayout.Button("转换坐标"))
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                OldItemCellToNew();
                OldMapCellToNew();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }
        
        
    }

    void OldMapCellToNew()
    {

        string mapParentPath = "Assets/Resources/Data/MapRoomData/";
        DirectoryInfo mapDir = new DirectoryInfo(mapParentPath);
        var files = mapDir.GetFiles("*.asset");
        foreach (var file in files)
        {
            string filePath = $"{mapParentPath}{file.Name}";
            MapRoomData mapRoomData=AssetDatabase.LoadAssetAtPath<MapRoomData>(filePath);
            List<MapCellData> mapCells = new List<MapCellData>(); 
            foreach(var cell in mapRoomData.mapCells)
            {
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        MapCellData mapCell = new MapCellData
                        {
                            coordinate = cell.coordinate * 2 + new int2(x, y),
                            isWalkable = cell.isWalkable
                        };
                        mapCells.Add(mapCell);
                    }
                }
            }
            mapRoomData.mapCells = mapCells;

            for(int i = 0; i < mapRoomData.mapItems.Count; i++)
            {
                var mapItem = mapRoomData.mapItems[i];
                mapItem.coordinate = mapItem.coordinate * 2;
                mapRoomData.mapItems[i] = mapItem;
            }

            mapRoomData.startCoordinate *= 2;
            mapRoomData.endCoordinate *= 2;

            EditorUtility.SetDirty(mapRoomData);
            AssetDatabase.SaveAssets();
        }
    }

    void OldItemCellToNew()
    {
        string itemParentPath = "Assets/Resources/Data/MapItemData/";
        DirectoryInfo itemDir = new DirectoryInfo(itemParentPath);
        var files = itemDir.GetFiles("*.asset");
        foreach(var file in files)
        {
            string filePath = $"{itemParentPath}{file.Name}";
            MapItemData itemData=AssetDatabase.LoadAssetAtPath<MapItemData>(filePath);

            if (itemData.colliderCells != null)
            {
                List<int2> colliderCells = new List<int2>();
                foreach (var cell in itemData.colliderCells)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            colliderCells.Add(cell * 2 + new int2(x, y));
                        }
                    }
                }

                itemData.colliderCells = colliderCells.ToArray();

            }
                

            if (itemData.triggerCells != null)
            {
                List<int2> triggerCells = new List<int2>();
                foreach (var cell in itemData.triggerCells)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            triggerCells.Add(cell * 2 + new int2(x, y));
                        }
                    }
                }
                itemData.triggerCells = triggerCells.ToArray();
            }
               

            if (itemData.playerTriggerCells != null)
            {
                List<int2> playerTriggerCells = new List<int2>();
                foreach (var cell in itemData.playerTriggerCells)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            playerTriggerCells.Add(cell * 2 + new int2(x, y));
                        }
                    }
                }
                itemData.playerTriggerCells = playerTriggerCells.ToArray();
            }
           

            EditorUtility.SetDirty(itemData);
            AssetDatabase.SaveAssets();
        }
    }
}
