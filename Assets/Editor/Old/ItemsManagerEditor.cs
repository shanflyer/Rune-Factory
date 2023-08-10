using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(ItemsManager))]
[ExecuteInEditMode]
public class ItemsManagerEditor :Editor {
    public override void OnInspectorGUI()
    {
        ItemsManager itemsManager = (ItemsManager)target;
        if (GUILayout.Button("保存道具数据", GUILayout.Width(150)))
        {
            itemsManager.DataToJson();
        }
        if (GUILayout.Button("读取道具数据", GUILayout.Width(150)))
        {
            itemsManager.JsonToData();
        }
        base.OnInspectorGUI();
    }
}
