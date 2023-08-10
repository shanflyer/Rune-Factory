using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(FestivalManager))]
[ExecuteInEditMode]

public class FestivalManagerEdit : Editor {
    public override void OnInspectorGUI()
    {
        FestivalManager festivalManager = (FestivalManager) target;
        if (GUILayout.Button("读取节日数据"))
        {
            festivalManager.JsonToData();
        }
        if (GUILayout.Button("保存节日数据"))
        {
            festivalManager.DataToJson();
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
