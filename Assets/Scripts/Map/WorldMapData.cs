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

#endif

    public string GetKey()
    {
        return name;
    }
}

[System.Serializable]
public struct WorldMap
{ 
    public MapRoomData mapRoomData;
    public int id;
    public int3 coordinate;
    public int eventId;
}

[System.Serializable]
public class MapLine
{
    public int instanceId;
    public bool zeroInit;
    public int map0, map1;
    public LinkMapCell cells0=new LinkMapCell(), cells1=new LinkMapCell();
    public int beforeActionId, afterActionId;

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
}

[System.Serializable]
public class LinkMapCell
{
    public List<Direction> directions;
    public List<int> girds;
    public int3 targetCell;
    public int beforAction, afterAction, checkAction;

}