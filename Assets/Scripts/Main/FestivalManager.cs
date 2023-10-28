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
    }
    public List<FestivalData> FestivalDatas;
     
    public List<FestivalData> customFestivalDatas;
   
    void LoadBrothDay()
    {
        FestivalData festivalData0 =
            new FestivalData
            {
                name = GameDataManager.instance.UserGameSaveData.playerData.name + LanguageManage.SwitchStr(" 的生日"),
                season = GameDataManager.instance.UserGameSaveData.playerData.brithDay.season,
                date = GameDataManager.instance.UserGameSaveData.playerData.brithDay.day,
                id = 8
            };
        FestivalDatas.Add(festivalData0);

        

    }
    void CreatNPCBrothDay()
    {
        List<GameTime> gameTimes=new List<GameTime>();
        CharacterSaveData characterSaveData = GameDataManager.instance.UserGameSaveData.playerData;

        FestivalData festivalData0 =
            new FestivalData
            {
                name = characterSaveData.name + LanguageManage.SwitchStr(" 的生日"),
                season = characterSaveData.brithDay.season,
                date = characterSaveData.brithDay.day,
                id = 8
            };
        FestivalDatas.Add(festivalData0);
        GameTime gameTime=new GameTime(0,festivalData0.season,festivalData0.date,0,0);
        gameTimes.Add(gameTime);
       

   
    }
   
}
