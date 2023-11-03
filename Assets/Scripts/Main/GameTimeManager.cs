using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

[System.Serializable]
public enum Week
{
    SunDay=0,
    Monday=1,
    TuesDay=2,
    WednesDay=3,
    ThursDay=4,
    FriDay=5
}
[System.Serializable]
public enum Season
{
  Default=0, 春=1,夏=2,秋=3,冬=4
}

[System.Serializable]
public struct GameDate : IReferenceData, INativeData
{
    public Season season;
    public int date;
    public UnsafeList<int> FestivaList;
    public UnsafeList<int> CustomFestival;
     
    public GameDate(Season _season,int _date,List<int> _festivals)
    {
        season = _season;
        date = _date;
        FestivaList=new UnsafeList<int>(4, Allocator.TempJob);
        foreach (var festival in _festivals)
        {
            FestivaList.Add(festival);
        }
        CustomFestival= new UnsafeList<int>(4, Allocator.TempJob);
    }
    public void Dispose()
    {
        FestivaList.Dispose();
        CustomFestival.Dispose();
    }
    public int Key => (int)season * 100 + date;
}
[System.Serializable]
public class GameTime
{ 
    public int year;
    public Season season;
    public int date;
    public int hour;
    public int minute; 
    public Week week;

    public GameTime()
    { 
        hour = 0;
        minute = 0;
        week=Week.SunDay; 
    }
    public int GetTimeKey()
    {
      return (year*1000+ (int)season * 100) + date;
    }
    public int GetTimeKeyNoYear()
    {
        return ((int)season * 100) + date;
    }
    public GameTime(int _year, Season _season, int _date, int _hour, int _minute)
    { 
        year = _year;
        season = _season;
        date = _date;
        hour = _hour;
        minute = _minute;
        TimeInit();
    }
    public void TimeRun()
    {
        minute++;
        TimeInit();
    }

    public void AddDate()
    {
        hour=30;
        TimeInit();
    }
    void TimeInit()
    {
        if (minute >= 60)
        {
            hour += minute / 60;
            minute = 0;
        }
        if (hour >= 24)
        {
            date += hour / 24;
            hour = hour % 24;
            GameActionManager.instance.QueueAction(new NewDay());
           
            GameComponentData.gameData.charactorTitleAction.AddSleepDays();   
            GameComponentData.gameData.gameManager.ChildDateCost();

            
        }
        if (date > 30)
        {
            int seasonId = (int)season;
            if (seasonId < 4)
            {
                seasonId++;
            }
            else if(seasonId == 4)
            {
                year++;
                seasonId = 1;
            }
            season = (Season)seasonId;
            date = 1; 
        }
        int x = date %6;
        week = (Week)x;
          
        if (LanguageManage.nowLanguage == SystemLanguage.Chinese)
        {
            InformationController.instance.AddInformation("*" + year + "年" + season + "之月" + date + "日");
        }
        else
        {
            InformationController.instance.AddInformation("*" + date + "," + LanguageManage.SwitchStr(season.ToString()) + "," + year + LanguageManage.SwitchStr("年"));
        }
    }
    public void Sleep()
    {
        minute = 0;
        hour = 6;
        date++;
        TimeInit();
    }

    public void RestToNight()
    {
        if (hour < 18)
        {
            minute = 0;
            hour = 18;
        }
    }
}
public class GameTimeManager : Singleton<GameTimeManager>
{
    private GameTime nowGameTime;
    public float timeRunScale=1;

    public string NowGameTime => LanguageManage.instance.GameTimeToString(nowGameTime);
    public int Year => nowGameTime.year;
    public Season Season => nowGameTime.season;
    public int Day => nowGameTime.date;

    MyNativeData<GameDate> gameDates = new MyNativeData<GameDate>();
    public List<GameDate> GetGameDataForSeason(Season season)
    {
        List<GameDate> results = new List<GameDate>();
        for(int i = 1; i <= 30; i++)
        {
            int key = (int)season * 100 + i;
            if(gameDates.GetData(key,out var gameDate))
            {
                results.Add(gameDate);
            }            
        }
        return results;
    }
    protected override void Clear()
    {
        base.Clear();
        gameDates.Dispose();
    }
    public override void Init()
    {
        base.Init();
        gameDates.Init(120);
        CreatData();
    }

    void ZeroGameTime()
    {

    }
    public string GameTimeToString()
    {
        string timeStr = nowGameTime.year.ToString();
        timeStr += "," + nowGameTime.season;
        timeStr += "," + nowGameTime.date;
        return timeStr;
    }

    public void StringToGameTime(string timeStr)
    {
        var x = timeStr.Split(',');
        nowGameTime.year = int.Parse(x[0]);
        nowGameTime.season = (Season) Enum.Parse(typeof(Season),x[1]);
        nowGameTime.date = int.Parse(x[2]); 
    }

    public int DaysToSave(string timeStr)
    {
        var x = timeStr.Split(',');
        int years = int.Parse(x[0]);
        int months = (int)Enum.Parse(typeof(Season), x[1]);
        int days = int.Parse(x[2]);
        return (years * 4 + months) * 30 + days;
    }

    public void CreatData()
    {
        //gameDates=new List<GameDate>();
        for (int i = 1; i <= 120; i++)
        {
            int seasonId = (i - 1) / 30 + 1;
            int date = i - (seasonId - 1) * 30;
            Season season = (Season)seasonId;
            List<FestivalData> festivals = FestivalManager.instance.FestivalDatas.FindAll(f => f.season == season &&
            f.date == date);
            List<int> festivalIds = new List<int>();
            foreach (var festivalData in festivals)
            {
                festivalIds.Add(festivalData.id);
            }
            GameDate gameDate = new GameDate(season, date, festivalIds);
            gameDates.AddData(gameDate);
        }
        //timeDisplayAction.UpdataTime();
    }

    IEnumerator TimeRunIEnumerator;
    public void StartTimeRun()
    {
        TimeRunIEnumerator = TimeRun();
        GameController.instance.StartCoroutine(TimeRunIEnumerator);
    }
    public void StopTimeRun()
    {
        if (TimeRunIEnumerator!=null)
        {
            GameController.instance.StopCoroutine(TimeRunIEnumerator);
        } 
    }

    public void NextDate()
    {
        nowGameTime.minute = 0;
        nowGameTime.AddDate();
    }

    IEnumerator TimeRun()
    {
        while (true)
        { 
            float waitTime=1.0f;
            if (timeRunScale > 0)
            {
                waitTime = waitTime / timeRunScale;
            }
            nowGameTime.TimeRun();
            //timeDisplayAction.UpdataTime();
            yield return new WaitForSeconds(waitTime);
        }
    }
	 
}
