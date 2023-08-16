using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class MapItemData:ScriptableObject,IGameData
{
    public int id;
    public string itemName;

    public GameObject itemObj;
  

    public int2[] triggerCells;
    public int defaultExit, defaultEnter;
    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        string path = $"{EditorDataPath.mapItemPrefabPath}{itemName}{".prefab"}";
        itemObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    public string GetKey()
    {
        return id.ToString();
    }
#endif
}