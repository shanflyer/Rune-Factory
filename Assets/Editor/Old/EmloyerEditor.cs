using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(EmployerManger))]
[ExecuteInEditMode]
public class EmployerEditor : Editor {
    public override void OnInspectorGUI()
    {
        EmployerManger employerManager = (EmployerManger)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            employerManager.DataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            employerManager
                .JsonToData();
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
