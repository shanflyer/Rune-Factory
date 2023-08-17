using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OldName;

[CustomEditor(typeof(CharactorDataAction))]
[ExecuteInEditMode]
public class CharactorDataActionEditor : Editor {

    public override void OnInspectorGUI()
    {
        CharactorDataAction charactorDataAction = (CharactorDataAction)target;
        
        if (GUILayout.Button("保存兵种数据", GUILayout.Width(150)))
        {
            charactorDataAction.ArmDataToJson();
        }
        if (GUILayout.Button("读取兵种数据", GUILayout.Width(150)))
        {
            charactorDataAction.JsonToArmData();
        }
        if (GUILayout.Button("保存职业数据", GUILayout.Width(150)))
        {
            charactorDataAction.ProfessionDataToJson();
        }
        if (GUILayout.Button("读取职业数据", GUILayout.Width(150)))
        {
            charactorDataAction.JsonToProfessionData();
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
