using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(InfluenceAction))]
[ExecuteInEditMode]
public class InfluenceActionEditor : Editor
{

    public override void OnInspectorGUI()
    {
        InfluenceAction influenceAction = (InfluenceAction)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            influenceAction.InfluenceDataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            influenceAction.JsonToInfluenceData();
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
