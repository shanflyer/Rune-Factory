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
    public TimelineAsset asset;
    public List<string> pathes;
}

[CreateAssetMenu(menuName ="电影数据")]
public class FilmData : ScriptableObject, IGameData, IReferenceData
{
    public string FilmName;
    public GameObject FilmObj;
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
            return default(TimelineAssetData);
        }
        timelineAssets.TryGetValue(assetName, out var timelineAsset);
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