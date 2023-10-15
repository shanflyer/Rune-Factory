using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Globalization;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(menuName ="Data/地图物体")]
[System.Serializable]
public class MapItemData:ScriptableObject,IGameData
{
    public int id;
    public string itemName;

    public GameObject itemObj;

    public int2[] colliderCells;
    public int2[] triggerCells;
    public int2[] playerTriggerCells;
    public int playerTriggerEvent;
    public List<int> operateIds = new List<int>();
    public List<OperateData> operateDatas = new List<OperateData>();
    public int defaultExit, defaultEnter;
    public string playerOperateInfo;
    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        string path = $"{EditorDataPath.mapItemPrefabPath}{itemName}{".prefab"}";
        itemObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        string operatePath = DataPath.GetDataPath(typeof(OperateData));
        operateDatas.Clear();
        foreach (var operate in operateIds)
        {
            operateDatas.Add(Resources.Load<OperateData>($"{operatePath}/{operate}"));
        }
    }

    public string GetKey()
    {
       return id.ToString();
    }
#endif
}