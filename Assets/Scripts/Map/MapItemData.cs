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

    public int2[] colliderCells;
    public int2[] triggerCells;
    public int2[] playerTriggerCells;

    public List<int> colliderGrids, triggerGrids, playerTriggerGrids;


    public int playerTriggerEvent;
    public List<int> operateIds = new List<int>();

    public GameActionData creatAction;

    public int defaultExit, defaultEnter;
    public bool displayTips = true;
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
        else if (itemObj != null)
        {
            objName = itemObj.name;
        }

        if (colliderCells != null)
            colliderGrids = GameCommon.CellToGrid(colliderCells.ToList());
        if (triggerCells != null)
            triggerGrids = GameCommon.CellToGrid(triggerCells.ToList());
        if (playerTriggerCells != null)
            playerTriggerGrids = GameCommon.CellToGrid(playerTriggerCells.ToList());

        colliderCells = null;
        triggerCells = null;
        playerTriggerCells = null;
    }

    public string GetKey()
    {
        return id.ToString();
    }

#endif
}