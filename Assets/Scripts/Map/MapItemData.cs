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
    public string objName;

    public GameObject itemObj;

    public int2[] colliderCells;
    public int2[] triggerCells;
    public int2[] playerTriggerCells;
    public int playerTriggerEvent;
    public List<int> operateIds = new List<int>(); 
    public int defaultExit, defaultEnter;
    public bool displayTips=true;
    public string playerOperateInfo;
    public string GetName()
    {
        return itemName;
    }
    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        if (!string.IsNullOrEmpty(objName))
        {
            string path = $"{EditorDataPath.mapItemPrefabPath}{objName}{".prefab"}";
            itemObj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
        else if(itemObj!=null)
        {
            objName = itemObj.name;
        }
         /*
        string operatePath = DataPath.GetDataPath(typeof(OperateData));
        operateDatas.Clear();
        foreach (var operate in operateIds)
        {
            operateDatas.Add(Resources.Load<OperateData>($"{operatePath}/{operate}"));
        }*/
    }

    public string GetKey()
    {
       return id.ToString();
    }
#endif
}