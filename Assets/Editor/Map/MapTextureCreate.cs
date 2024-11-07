using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class MapTextureCreate : MyEditor
{
    static MapTextureCreate Instance;
    [MenuItem("工具/地图TileMap生成")]
    public static void WindowShow()
    {
        Instance = CreateWindow<MapTextureCreate>("地图TileMap生成");
        Instance.minSize = new Vector2(360, 480);
        Instance.maxSize = new Vector2(360, 480);
        Instance.ShowAuxWindow();
    }
    List<Tilemap> tilemaps = new List<Tilemap>();
    private void OnGUI()
    {
        
    }
}
