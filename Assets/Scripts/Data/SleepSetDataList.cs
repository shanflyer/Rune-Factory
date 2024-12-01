using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
    public List<SleepSetData> GetNowSleepSetData(int hour,int gameDay)
    {
        List<SleepSetData> nowSleepSetDatas = new List<SleepSetData>();
        for(int i = 0; i < DataList.Length; i++)
        {
            SleepSetData sleepSetData = DataList[i];
            if (sleepSetData.gameDay > gameDay)
            {
                continue;
            }
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
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        for(int i = 0; i < DataList.Length; i++)
        {
            string referencePath = $"Assets/Resources/Reference/{DataList[i].iconName}.asset";
            DataList[i].icon = AssetDatabase.LoadAssetAtPath<SpriteResourceRenference>(referencePath).sprite;
        }
    }
#endif

}
[Serializable]
public struct SleepSetData : IReferenceData, IGameData
{
    public string text;
    public bool SleepToTime;
    public int gameDay;
    public int hour;
    public int minute; 
    public string iconName;
    public Sprite icon;
    public int startHour, endHour;
    public string GetKey()
    {
        return text;
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {

    }
#endif

    public override string ToString()
    {
        return text;
    }
}