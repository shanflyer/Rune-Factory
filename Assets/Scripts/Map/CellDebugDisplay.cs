using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using OfficeOpenXml.Style;
#if UNITY_EDITOR
using UnityEditor;
public class CellDebugDisplay : MonoBehaviour
{
    [SerializeField]
    Tilemap tilemap;
    [SerializeField]
    TileBase colliderTile, defaultTile, triggerTile;

    public static CellDebugDisplay Instance;
    private void OnEnable()
    {
        Instance = this;
    }
    public void DisplayPath(int2[] pathNodes)
    {
        tilemap.ClearAllTiles();
        TileBase tileBase = defaultTile;
        for(int i=0;i<pathNodes.Length;i++)
        {
            var node = pathNodes[i];
            tilemap.SetTile(new Vector3Int(node.x, node.y, 0), tileBase);
        } 
    }
    public void RefreshDisplayMapCell()
    {
        tilemap.ClearAllTiles();
        var roomCoordinate=MapCellController.instance.GetRoomCoordinate(WorldMapObjManager.instance.displayMap);
        var cellData = MapCellController.instance.GetAllCellData(WorldMapObjManager.instance.displayMap); 
        for (int i = 0; i < cellData.Count; i++)
        {
            var cell = cellData[i];
            TileBase tileBase = defaultTile;
            if (cell.z == 0)
            {
                tileBase = colliderTile;
            }
            tilemap.SetTile(new Vector3Int(cell.x, cell.y, +roomCoordinate.z), tileBase);
            //tilemap.SetTile(new Vector3Int(cell.x+roomCoordinate.x, cell.y + roomCoordinate.y, +roomCoordinate.z), tileBase);
        }
        var playerTrigger = MapCellController.instance.GetPlayerTrigger(WorldMapObjManager.instance.displayMap);
        for (int i = 0; i < playerTrigger.triggerAreas.Length; i++)
        {
            var area = playerTrigger.triggerAreas[i];
            foreach(var cell in area.cells)
            {
                tilemap.SetTile(new Vector3Int(cell.x, cell.y, +roomCoordinate.z), triggerTile);
            } 
        }


        tilemap.RefreshAllTiles();
    }
}
[CustomEditor(typeof(CellDebugDisplay))]
public class CellDebugDisplayEditor : Editor
{
    CellDebugDisplay debugDisplay
    {
        get
        {
            return target as CellDebugDisplay;
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("刷新格子显示"))
        {
            debugDisplay.RefreshDisplayMapCell();
        }
    }
}
#endif

