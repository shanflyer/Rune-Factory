using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(CharactorTitleAction))]
[ExecuteInEditMode]
public class CharactorTitlesEditor : Editor {
    public override void OnInspectorGUI()
    {
        CharactorTitleAction charactorTitleAction = (CharactorTitleAction)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            charactorTitleAction.DataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            charactorTitleAction.JsonToData();
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
