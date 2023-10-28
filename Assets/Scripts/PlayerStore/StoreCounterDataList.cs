using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[CreateAssetMenu(menuName ="Data/玩家柜台数据")]
public class StoreCounterDataList : ScriptableObject, IGameData, IDataArray<StoreCounterData>
{
    [SerializeField]
    StoreCounterData[] storeCounterDatas;
    public StoreCounterData[] DataList => storeCounterDatas;

    public string GetKey()
    {
        return "StoreCounterDataList";
    }

    public void SetReferenceData()
    { 
    }
}
[System.Serializable]
public struct StoreCounterData :  IGameData
{
    public string counterName;
    public int id; 
    public int linkItem;
    public Vector3 offset;
    public List<ItemType> itemTypes;
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
    public void SetReferenceData()
    { 
    }
}