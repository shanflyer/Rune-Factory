using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(ShopManager))]
[ExecuteInEditMode]
public class ShopDataEditor : Editor {
    public override void OnInspectorGUI()
    {
        ShopManager shopManager = (ShopManager)target;
        if (GUILayout.Button("保存数据", GUILayout.Width(100)))
        {
            shopManager.DataToJson();
        }
        if (GUILayout.Button("读取数据", GUILayout.Width(100)))
        {
            shopManager.JsonToData();
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
