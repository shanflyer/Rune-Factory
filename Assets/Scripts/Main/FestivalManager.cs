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

        foreach (var npcx in GameComponentData.gameData.NpcManager.Npcxs)
        {
            FestivalData festivalData = new FestivalData
            {
                date = npcx.npcData.brothDate,
                season = npcx.npcData.brothSeason,
                id = npcx.id,
                name = npcx.npcData.name + LanguageManage.SwitchStr(" 的生日"),
                festivalType = FestivalType.纪念
            };
            FestivalDatas.Add(festivalData);
        }

    }
    void CreatNPCBrothDay()
    {
        List<GameTime> gameTimes=new List<GameTime>();
        FestivalData festivalData0 =
            new FestivalData
            {
                name = PlayerDate.playerName + LanguageManage.SwitchStr(" 的生日"),
                season = PlayerDate.season,
                date = PlayerDate.date,
                id = 8
            };
        FestivalDatas.Add(festivalData0);
        GameTime gameTime=new GameTime(0,festivalData0.season,festivalData0.date,0,0);
        gameTimes.Add(gameTime);
       

        foreach (var npcManagerNpcData in GameComponentData.gameData.NpcManager.NpcDatas)
        {
            GameTime x;
            
            while (true)
            {
                Season season = (Season)Random.Range(1, 5);
                int day = Random.Range(1, 31);
                x = new GameTime(0, season, day, 0, 0);
                if (!gameTimes.Contains(x))
                {
                    break;
                }
            }
            gameTimes.Add(x);
            FestivalData festivalData = new FestivalData
            {
                date = x.gameDate.date,
                season = x.gameDate.season,
                id = npcManagerNpcData.id,
                name = npcManagerNpcData.name + LanguageManage.SwitchStr(" 的生日"),
                festivalType = FestivalType.纪念
            };
            FestivalDatas.Add(festivalData);
        }
    }
   
}
