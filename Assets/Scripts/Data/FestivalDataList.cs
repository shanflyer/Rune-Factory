using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FestivalDataList: ScriptableObject,IGameData,IDataArray<FestivalData>
{
    public FestivalData[] festivalDatas;

    public FestivalData[] DataList => festivalDatas;

    public override string ToString()
    {
        return "FestivalDataList";
    }
    public string GetKey()
    {
        return "FestivalDataList";
    }

    public void SetReferenceData()
    {
    }
}
[System.Serializable]
public enum FestivalType
{
    比赛 = 1,
    售卖 = 2,
    纪念 = 3,
    狂欢 = 4
}
[System.Serializable]
public struct FestivalData : IGameData, IReferenceData
{
    public string name;
    public int id;
    public Season season;
    public int date;
    public FestivalType festivalType;
    public string value;
    public string text;
#if UNITY_EDITOR
    public void SetReferenceData()
    {
    }
#endif
    public string GetKey()
    {
        return id.ToString();
    }
}
