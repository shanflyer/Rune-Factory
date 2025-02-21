using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Linq;

#if UNITY_EDITOR

using UnityEditor;

#endif

[CreateAssetMenu(menuName = "Data/地图物体")]
[System.Serializable]
public class MapItemData : ScriptableObject, IGameData
{
    public int id;
    public string itemName;
    public string objName;

    public GameObject itemObj;

    public List<int> colliderGrids, triggerGrids, playerTriggerGrids;
    public bool isPlayerForwardTrigger;

    public int playerTriggerEvent;
    public List<int> operateIds = new List<int>();

    public GameActionData creatAction;

    public int defaultExit, defaultEnter;
    public bool displayTips = true;
    public string playerOperateInfo;

    public Vector2 offsetLinkPos;
    public Vector2 leftLinkPos, rightLinkPos;
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