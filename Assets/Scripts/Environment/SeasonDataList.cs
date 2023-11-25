using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "Datas/季节数据")]
public class SeasonDataList : ScriptableObject, IGameData, IDataArray<SeasonData>
{
    [SerializeField]
    SeasonData[] SeasonDatas;
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

public struct SeasonData:IGameData
{
    public Season season;
    public int sunupHour, sunupMinute;
    public int sundownHour, sundownMinute;
    public int dayWeather, nightWeather;
    public int dayEnvironmentDataId, nightEnvironmentDataId;
     
    public string GetKey()
    {
        return season.ToString();
    }

    public void SetReferenceData()
    {
        
    }
}