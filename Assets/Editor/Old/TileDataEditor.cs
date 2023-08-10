using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MapDataAction))]
[ExecuteInEditMode]
public class TileDataEditor : Editor {

    public override void OnInspectorGUI()
    {
        MapDataAction mapDataAction = (MapDataAction)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            mapDataAction.TileDataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            mapDataAction.JsonToTileData();
        }
        base.OnInspectorGUI();
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
