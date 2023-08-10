using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(FilmManager))]
[ExecuteInEditMode]
public class FilmmanagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FilmManager filmManager = (FilmManager) target;
        if (GUILayout.Button("保存film数据"))
        {
            filmManager.DataToJson();
        }
        if (GUILayout.Button("读取film数据"))
        {
            filmManager.JsonToData();
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
