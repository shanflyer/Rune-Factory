using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[CreateAssetMenu(menuName ="Data/玩家柜台数据")]
public class StoreCounterDataList : ScriptableObject, IGameData, IDataArray<StoreCounterData>
{
    [SerializeField]
    public StoreCounterData[] storeCounterDatas;
    public StoreCounterData[] DataList => storeCounterDatas;

    public string GetKey()
    {
        return "StoreCounterDataList";
    }
    public override string ToString()
    {
        return "StoreCounterDataList";
    }
    public void SetReferenceData()
    {
    }
}
[System.Serializable]
public class StoreCounterData :  IGameData
{
    public string counterName;
    public int id;
    public Vector3 offset;
    public List<int> itemTypes;
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
