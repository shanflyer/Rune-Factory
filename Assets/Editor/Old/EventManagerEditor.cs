using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OldName;
[CustomEditor(typeof(EventManager))]
[ExecuteInEditMode]
public class EVentManagerEditor : Editor {
    public override void OnInspectorGUI()
    {
        EventManager gameEndManager = (EventManager)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            gameEndManager.DataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            gameEndManager.JsonToData();
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
