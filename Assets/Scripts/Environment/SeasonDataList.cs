using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Datas/季节数据")]
public class SeasonDataList : ScriptableObject, IGameData, IDataArray<SeasonData>
{
    [SerializeField]
    private SeasonData[] SeasonDatas;

    public SeasonData[] DataList => SeasonDatas;

    public override string ToString()
    {
        return "SeasonDataList";
    }

    public string GetKey()
    {
        return "SeasonDataList";
    }

    public void SetReferenceData()
    {
    }
}

[Serializable]
public struct SeasonData : IGameData
{
    public Season season;
    public int sunupHour, sunupMinute;
    public int sundownHour, sundownMinute;
    public int WeatherCurveId,WeatherCurveId1;
    public string dayEnvironmentDataName, duskEnvironmentDataName, dawnEnvironmentDataName, nightEnvironmentDataName;

    public string GetKey()
    {
        return season.ToString();
    }

    public void SetReferenceData()
    {
    }
}