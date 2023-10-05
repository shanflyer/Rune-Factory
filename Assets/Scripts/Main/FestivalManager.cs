using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;

[System.Serializable]
public enum FestivalType
{
    比赛=1,
    售卖=2,
    纪念=3,
    狂欢=4
}
[System.Serializable]
public struct FestivalData : IGameData
{
    public string name;
    public string englishName;
    public int id;
    public Season season;
    public int date;
    public FestivalType festivalType;
    public string value;
    public string text;
    public string englishText;
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
public class FestivalManager : Singleton<FestivalManager>
{
    public override async void Init()
    {
        base.Init();

        customFestivalDatas = await GameDataManager.instance.GetAllAsyncData<FestivalData>();
        CreatNPCBrothDay();
        LoadBrothDay();
    }
    public List<FestivalData> FestivalDatas;
     
    public List<FestivalData> customFestivalDatas;
   
    void LoadBrothDay()
    {
        FestivalDatas = new List<FestivalData>();
        FestivalData festivalData0 =
            new FestivalData
            {
                name = GameComponentData.gameData.gameManager.gamePlayer.name + LanguageManage.SwitchStr(" 的生日"),
                season = GameComponentData.gameData.gameManager.gamePlayer.season,
                date = GameComponentData.gameData.gameManager.gamePlayer.date,
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
