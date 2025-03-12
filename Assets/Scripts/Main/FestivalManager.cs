using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.Mathematics;

public class FestivalManager : Singleton<FestivalManager>
{
    public override async void Init()
    {
        base.Init();

        var _customFestivalDatas = await GameDataManager.instance.GetAllAsyncData<FestivalData>();
        FestivalDatas = new Dictionary<int2, List<FestivalData>>();
        customFestivalDatas = new Dictionary<int2, List<FestivalData>>();

        for(int i = 0; i < _customFestivalDatas.Count; i++)
        {
            var data = _customFestivalDatas[i];
            int2 key = new int2((int)data.season, data.date);
            if(!customFestivalDatas.TryGetValue(key,out var festivalDatas))
            {
                festivalDatas = new List<FestivalData>();
                customFestivalDatas.Add(key, festivalDatas);
            }
            festivalDatas.Add(data);
        }
        LoadBrothDay();
        GameTimeManager.instance.CreatData();
    }
    public  Dictionary<int2,List<FestivalData>> FestivalDatas;

    public Dictionary<int2, List<FestivalData>> customFestivalDatas;
   
    void LoadBrothDay()
    {
        /*
        if (GameDataSaveManager.instance.IsZeroGameSave)
        {
            return;
        }*/
        FestivalData festivalData0 =
            new FestivalData
            {
                name = GameDataSaveManager.instance.UserGameSaveData.playerData.name + LanguageManage.SwitchStr(" 的生日"),
                season = GameDataSaveManager.instance.UserGameSaveData.playerData.brithDay.season,
                date = GameDataSaveManager.instance.UserGameSaveData.playerData.brithDay.day,
                id = 8
            };
        int2 key = new int2((int)festivalData0.season, festivalData0.date);
        if (!FestivalDatas.TryGetValue(key, out var festivalDatas))
        {
            festivalDatas = new List<FestivalData>();
            FestivalDatas.Add(key, festivalDatas);
        }
        festivalDatas.Add(festivalData0);  
    }
    
    public void AddNPCBrothDay(string name,Season season,int date,int id)
    {
        FestivalData festivalData = new FestivalData
        {
            name = LanguageManage.SwitchStr(name, " 的生日"),
            season = season,
            date = date,
            id = id
        };
        int2 key = new int2((int)season,date);
        if (!FestivalDatas.TryGetValue(key, out var festivalDatas))
        {
            festivalDatas = new List<FestivalData>();
            FestivalDatas.Add(key, festivalDatas);
        }
        festivalDatas.Add(festivalData); 
    }
   
}
