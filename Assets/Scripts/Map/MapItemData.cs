using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR

using UnityEditor;

#endif

[CreateAssetMenu(menuName = "Data/地图物体")]
[Serializable]
public class MapItemData : ScriptableObject, IGameData
{
    public int id;
    public string itemName;
    public string objName;
    public bool zOffset;

    public GameObject itemObj;

    public List<int> colliderGrids, triggerGrids, playerTriggerGrids;
    public bool isPlayerForwardTrigger;

    public string maskObj;

    public int playerTriggerEvent;
    public List<int> operateIds = new List<int>();

    public GameActionData creatAction;

    public int defaultExit, defaultEnter;
    public bool displayTips = true;
    public string playerOperateInfo;

    public Vector3 offsetLinkPos;
    public Vector3 leftLinkPos, rightLinkPos;
    public Direction linkDirection;
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
        else if (itemObj != null)
        {
            objName = itemObj.name;
        }
    } 

#endif
    public string GetKey()
    {
        return id.ToString();
    }
}