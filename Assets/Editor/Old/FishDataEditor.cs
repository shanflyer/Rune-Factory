using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(FishManager))]
[ExecuteInEditMode]
public class FishDataEditor : Editor {
    public override void OnInspectorGUI()
    {
        FishManager monsterManager = (FishManager)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            monsterManager.DataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            monsterManager.JsonToData();
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
