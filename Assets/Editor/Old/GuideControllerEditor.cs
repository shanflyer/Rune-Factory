using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(GuideController))]
[ExecuteInEditMode]
public class GuideControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GuideController guideController = (GuideController) target;
        if (GUILayout.Button("保存指引数据"))
        {
            guideController.SaveGuidData();
        }
        if (GUILayout.Button("读取指引数据"))
        {
            guideController.LoadGuidData();
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
