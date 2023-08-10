using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(TalkTextsManager))]
[ExecuteInEditMode]
public class TalksManagerEditor : Editor {
    public override void OnInspectorGUI()
    {
        TalkTextsManager talkManager = (TalkTextsManager)target;
        if (GUILayout.Button("保存对话数据",GUILayout.Width(150)))
        {
            talkManager.DataToJson();
        }
        if (GUILayout.Button("读取对话数据",GUILayout.Width(150)))
        {
            talkManager.JsonToData();
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
