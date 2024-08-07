
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;

#endif 
 
[Serializable]
public struct MapNpcData
{
    public string npcName;
    public int id;
    public int dataId;
    public bool initialBegin;
    public int beginMap;
    public int2 beginCoordinate;

    public string GetKey()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    { 
    }
}
public class MapNpcDataList : ScriptableObject, IGameData
{
    [Serializable]
    public struct DataList
    {
        public List<MapNpcData> datas;
    }
#if UNITY_EDITOR
    private MapNpcData[] mapNpcDatas; 
#endif
   
    public IntIntDictionary keys=new IntIntDictionary(); 
    public List<DataList> datas = new List<DataList>();
    public List<MapNpcData> GetMapNPCDatas(int mapId)
    {
        if(keys.TryGetValue(mapId,out var index))
        {
            return datas[index].datas;
        }
        return null;
    }
    public string GetKey()
    {
        return "MapNpcDataList";
    }
    public override string ToString()
    {
        return "MapNpcDataList";
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        keys.Clear();
        datas.Clear();
        for(int i = 0; i < mapNpcDatas.Length; i++)
        {
            var mapNpcData = mapNpcDatas[i];
            if(!keys.TryGetValue(mapNpcData.beginMap,out var index))
            {
                keys.Add(mapNpcData.beginMap, this.datas.Count);
                var _datas = new List<MapNpcData>();
                this.datas.Add(new DataList { datas = _datas }) ;
                _datas.Add(mapNpcData);
            }
            else
            {
                datas[index].datas.Add(mapNpcData);
            } 
        }
    }
#endif

}
