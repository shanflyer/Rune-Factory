using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[ExecuteInEditMode]
[CustomEditor(typeof(VillageManager))]
public class VillageManagerEdit : Editor
{
    public override void OnInspectorGUI()
    {
        VillageManager villageManager = (VillageManager) target;
        if (GUILayout.Button("读取村庄数据"))
        {
            villageManager.JsonToData();
        }
        if (GUILayout.Button("保存村庄数据"))
        {
            villageManager.DataToJson();
        }
        base.OnInspectorGUI();
    }
}
