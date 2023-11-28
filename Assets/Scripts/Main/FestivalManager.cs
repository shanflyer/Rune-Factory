using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;


public class FestivalManager : Singleton<FestivalManager>
{
    public override async void Init()
    {
        base.Init();

        customFestivalDatas = await GameDataManager.instance.GetAllAsyncData<FestivalData>();
        FestivalDatas = new List<FestivalData>();
        CreatNPCBrothDay();
        LoadBrothDay();
        GameTimeManager.instance.CreatData();
    }
    public List<FestivalData> FestivalDatas;
     
    public List<FestivalData> customFestivalDatas;
   
    void LoadBrothDay()
    {
        if (GameDataSaveManager.instance.IsZeroGameSave)
        {
            return;
        }
        FestivalData festivalData0 =
            new FestivalData
            {
                name = GameDataSaveManager.instance.UserGameSaveData.playerData.name + LanguageManage.SwitchStr(" 的生日"),
                season = GameDataSaveManager.instance.UserGameSaveData.playerData.brithDay.season,
                date = GameDataSaveManager.instance.UserGameSaveData.playerData.brithDay.day,
                id = 8
            };
        FestivalDatas.Add(festivalData0);  
    }
    void CreatNPCBrothDay()
    {
        if (GameDataSaveManager.instance.IsZeroGameSave)
        {
            return;
        }
        CharacterSaveData characterSaveData = GameDataSaveManager.instance.UserGameSaveData.playerData;

        FestivalData festivalData0 =
            new FestivalData
            {
                name = characterSaveData.name + LanguageManage.SwitchStr(" 的生日"),
                season = characterSaveData.brithDay.season,
                date = characterSaveData.brithDay.day,
                id = 8
            };
        FestivalDatas.Add(festivalData0);
    }
   
}
