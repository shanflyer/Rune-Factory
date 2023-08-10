using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MapEditAction))]
[ExecuteInEditMode]
public class MapDataEditor : Editor {

    public override void OnInspectorGUI()
    {
        MapEditAction mapDataAction = (MapEditAction)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            mapDataAction.MapDataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            mapDataAction.JsonToMapData();
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
