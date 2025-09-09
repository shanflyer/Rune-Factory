using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/世界数据")]
public class WorldMapData : ScriptableObject, IGameData
{
    public int defaultMap;
    public List<MapLine> mapLines = new List<MapLine>();

    public IntWorldMapDictionary worldMapDic;
#if UNITY_EDITOR

    public void SetReferenceData()
    {
      
    }
    public StringStringDictionary GetDataDic()
    {
        StringStringDictionary dic = new StringStringDictionary();
        using(var e = worldMapDic.GetEnumerator())
        {
            while (e.MoveNext())
            {
                dic.Add(e.Current.Key.ToString(), e.Current.Value.mapRoomData.name);
            }
        }
        return dic;
    }
#endif

    public string GetKey()
    {
        return name;
    }
}

[Serializable]
public struct WorldMap
{ 
    public MapRoomData mapRoomData;
    public int id;
    public int3 coordinate;
    public int eventId;
}

[Serializable]
public class MapLine
{
    public int instanceId;
    public bool zeroInit;
    public int map0, map1;
    public LinkMapCell cells0=new LinkMapCell(), cells1=new LinkMapCell();
    public int afterActionId;
    public bool testInit;
    public MapLine()
    {

    }
    public int2 center0
    {
        get
        {
            return cells1.targetCell.xy;
        }
    }

    public int2 center1
    {
        get
        {
            return cells0.targetCell.xy;
        }
    }

    public int2 start0 => GameCommon.GridCenter(cells0.girds);

    public int2 start1 => GameCommon.GridCenter(cells1.girds);
}

[Serializable]
public class LinkMapCell
{
    public List<Direction> directions=new List<Direction>();
    public List<int> girds=new List<int>();
    public int3 targetCell;
    public int afterAction;

}