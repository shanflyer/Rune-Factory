using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(FormulaAction))]
[ExecuteInEditMode]
public class FormulaEditor : Editor {
    public override void OnInspectorGUI()
    {
        FormulaAction formulaAction = (FormulaAction)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            formulaAction.DataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            formulaAction.JsonToData();
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
