using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
public class CellDebugDisplay : MonoBehaviour
{
    [SerializeField]
    Tilemap tilemap;
    [SerializeField]
    TileBase colliderTile, defaultTile, triggerTile;

    public void RefreshDisplayMapCell()
    {
        tilemap.ClearAllTiles();
        var roomCoordinate=MapCellController.instance.GetRoomCoordinate(WorldMapManager.instance.displayMap);
        var data = MapCellController.instance.GetRoomCellData(WorldMapManager.instance.displayMap);
        var cellData = data.GetAllCellData();
        for (int i = 0; i < cellData.Count; i++)
        {
            var cell = cellData[i];
            TileBase tileBase = defaultTile;
            if (cell.z == 0)
            {
                tileBase = colliderTile;
            }
            tilemap.SetTile(new Vector3Int(cell.x+roomCoordinate.x, cell.y + roomCoordinate.y, +roomCoordinate.z), tileBase);
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

