using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(PlantAction))]
[ExecuteInEditMode]
public class PlantDataEditor : Editor {
    public override void OnInspectorGUI()
    {
        PlantAction plantAction = (PlantAction)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            plantAction.DataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            plantAction.JsonToData();
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
