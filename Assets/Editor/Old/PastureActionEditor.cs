using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(PastureAction))]
[ExecuteInEditMode]
public class PastureActionEditor : Editor {
    public override void OnInspectorGUI()
    {
        PastureAction pastureAction = (PastureAction)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            pastureAction.DataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            pastureAction
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
