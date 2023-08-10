using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(BattleMapAction))]
[ExecuteInEditMode]
public class BattleMapEditor : Editor {
    public override void OnInspectorGUI()
    {
        BattleMapAction battleMapAction = (BattleMapAction)target;

        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            battleMapAction
                
                .JsonToData();
        }
        base.OnInspectorGUI();
        if (GUILayout.Button("读取怪物群数据", GUILayout.Width(100)))
        {
            battleMapAction.JsonToMonsterGropData();
        }
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
