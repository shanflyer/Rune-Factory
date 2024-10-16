 
using System;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct MapItem
{
    public int id;
    public int instanceId;
    public int2 coordinate;
    public int animationKey;
    public int blindHomeEquipment;

    public List<MapItemEventReferenceData> eventReferenceDatas;
}

[System.Serializable]
public struct MapCellData
{
    public int2 coordinate;
    public bool isWalkable;
}

public enum FlowCameraType
{
    Default, FlowX, FlowY
}

public enum BehaviorAreaType
{
    创建, 聚集, 消失,特殊
}

[Serializable]
public class NpcBehaviorArea
{
    public int Name;
    public int2 pos;
    public List<int> grids = new List<int>();
    public BehaviorAreaType behaviorAreaType;
}
[Serializable]
public class SpecialNpcBehaviorArea
{
    public int Name;
    public int2 pos;
    public List<int> grids = new List<int>();
    public int tempCreatId;
    public Direction fixedDirection;
}
[Serializable]
public struct MapBGSData
{
    public BGS bgs;
    public AnimationCurve timeCurve;
    public AnimationCurve seasonCurve;
    public AnimationCurve waterFallCurve;

    public float GetValue(float seasonValue, float timeValue, float waterFallValue)
    {
        return seasonCurve.Evaluate(seasonValue) * timeCurve.Evaluate(timeValue) * waterFallCurve.Evaluate(waterFallValue);
    }
}

public class MapRoomData : ScriptableObject, IGameData
{
    public string roomName;
    public List<int> barrierGrids=new List<int>();

    public List<MapItem> mapItems = new List<MapItem>();
    public int2 startCoordinate, endCoordinate;
    public GameObject mapObj;
    public string dayEnvironmentDataName, duskEnvironmentDataName, dawnEnvironmentDataName, nightEnvironmentDataName;
    public bool displaySky = true;
    public bool displaySunlight = false;
    public bool fixedCamera;
    public FlowCameraType flowCameraType;
    public Vector3 fixedCameraPos;
    public int skyBackGroundId;
    public int creatTempCharacterId;
    public float fixedSeason;
    public WeatherDisplayType weatherDisplayType;

    public List<MapBGSData> mapBGSDatas = new List<MapBGSData>();
     
    public bool autoCreatTempNpc;
    public bool tempNpcPrewarm;
    public List<NpcBehaviorArea> npcBehaviorAreas = new List<NpcBehaviorArea>();
    public List<SpecialNpcBehaviorArea> specialNpcBehaviorAreas = new List<SpecialNpcBehaviorArea>();

    public void SetBGS(float seasonValue,float timeValue,float waterFallValue)
    {
        List<float3> bgs=new List<float3>();
        for(int i=0;i<mapBGSDatas.Count;i++)
        {
            float value=mapBGSDatas[i].GetValue(seasonValue, timeValue, waterFallValue);
            if (value > 0)
            {
                bgs.Add(new float3((int)mapBGSDatas[i].bgs, value, 1)); 
            }
        }
        AudioController.instance.PlayAudio(bgs,AudioClearType.All,true,"Map");
    }
    public bool CheckBoundary(int2 coordinate)
    {
        if (coordinate.x <= endCoordinate.x && coordinate.x >= startCoordinate.x
            && coordinate.y <= endCoordinate.y && coordinate.y >= startCoordinate.y)
        {
            return true;
        }
        return false;
    }
#if UNITY_EDITOR

    public void SetReferenceData()
    {
        
    }

#endif
    public string GetName()
    {
        return roomName;
    }

    public string GetKey()
    {
        return roomName;
    }
}