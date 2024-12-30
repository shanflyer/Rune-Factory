using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
[Serializable]
public struct TimelineAssetData
{
    public bool needFilmUI;
    public bool displayCharacter;
    public bool hideCameraLimit;
    public TimelineAsset asset;
    public List<string> pathes;
}

[CreateAssetMenu(menuName ="电影数据")]
public class FilmData : ScriptableObject, IGameData, IReferenceData
{
    public string FilmName;
    public GameObject FilmObj;
    public bool stopTimeRun;
    
    [SerializeField]
    public List<TimelineAssetData> TimelineAssets;
    public StringTimelineAssetDataDictionary timelineAssets =new StringTimelineAssetDataDictionary();
    public  string GetKey()
    {
       return FilmName;
    }
    public override string ToString()
    {
        return FilmName;
    }
    public TimelineAssetData GetTimeLineAsset(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            if (TimelineAssets.Count != 0)
            {
                return TimelineAssets[0];
            }
            else
            {
                return default(TimelineAssetData);
            }
            
        }
        if( timelineAssets.TryGetValue(assetName, out var timelineAsset))
        {
            //FilmName = assetName;
        }
        return timelineAsset;
    }

    public void SetReferenceData()
    {
        timelineAssets.Clear();
        for(int i=0;i<TimelineAssets.Count;i++)
        {
            var timelineAsset= TimelineAssets[i];
            timelineAssets.Add(timelineAsset.asset.name, timelineAsset);
        }
    }
}