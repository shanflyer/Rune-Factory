using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(PassDataManager))]
[ExecuteInEditMode]
public class PassDataEditor : Editor {
    public override void OnInspectorGUI()
    {
        PassDataManager passData = (PassDataManager)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(150)))
        {
            passData.DataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(150)))
        {
            passData.JsonToData();
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
