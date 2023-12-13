using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Datas/ÀØ√ﬂ…Ë÷√ ˝æ›")]
public class SleepSetDataList : ScriptableObject, IGameData, IDataArray<SleepSetData>
{
    [SerializeField]
    public SleepSetData[] sleepSetDatas;
    public SleepSetData[] DataList => sleepSetDatas;

    public override string ToString()
    {
        return "SleepSetDataList";
    }
    public string GetKey()
    {
        return "SleepSetDataList";
    }
    public List<SleepSetData> GetNowSleepSetData(int hour)
    {
        List<SleepSetData> nowSleepSetDatas = new List<SleepSetData>();
        for(int i = 0; i < DataList.Length; i++)
        {
            SleepSetData sleepSetData = DataList[i];
            if (!sleepSetData.SleepToTime)
            {
                nowSleepSetDatas.Add(sleepSetData);
            }
            else
            {
                if (sleepSetData.startHour > sleepSetData.endHour)
                {
                    if (hour >= sleepSetData.startHour || hour < sleepSetData.endHour)
                    {
                        nowSleepSetDatas.Add(sleepSetData);
                    }
                }else if (sleepSetData.startHour <= hour && sleepSetData.endHour > hour)
                {
                    nowSleepSetDatas.Add(sleepSetData);
                }
            }
            
        }
        return nowSleepSetDatas;
    }
    public void SetReferenceData()
    {
         
    }
}
[Serializable]
public struct SleepSetData : IReferenceData, IGameData
{
    public string text;
    public bool SleepToTime;
    public int hour;
    public int minute; 
    public string icon;
    public int startHour, endHour;
    public string GetKey()
    {
        return text;
    }

    public void SetReferenceData()
    {
       
    }
    public override string ToString()
    {
        return text;
    }
}