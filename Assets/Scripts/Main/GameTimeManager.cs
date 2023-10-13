using System;
using System.Collections;
using System.Collections.Generic;
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
    春=1,夏=2,秋=3,冬=4
}

[System.Serializable]
public class GameDate:IReferenceData
{
    public int year;
    public Season season;
    public int date;
    [HideInInspector]
    public List<int> FestivaList;
    [HideInInspector]
    public List<int> CustomFestival;
    public GameDate()
    {
        year = 1330;
        season=Season.春;
        date = 1;
        FestivaList=new List<int>();
        CustomFestival=new List<int>();
    }
    public GameDate(GameDate gameDate)
    {
        year = gameDate.year;
        season = gameDate.season;
        date = gameDate.date;
        FestivaList = new List<int>();
        foreach (var i in gameDate.FestivaList)
        {
            FestivaList.Add(i);
        }
        CustomFestival = new List<int>();
        foreach (var i in gameDate.CustomFestival)
        {
            CustomFestival.Add(i);
        }
    }
    public GameDate(int _year,Season _season,int _date,List<int> _festivals)
    {
        year = _year;
        season = _season;
        date = _date;
        FestivaList=new List<int>();
        foreach (var festival in _festivals)
        {
            FestivaList.Add(festival);
        }
        CustomFestival=new List<int>();
    }
}
[System.Serializable]
public class GameTime
{
    public GameDate gameDate;
    public int hour;
    public int minute;
    [HideInInspector]
    public Week week;

    public GameTime()
    {
        gameDate=new GameDate();
        hour = 0;
        minute = 0;
        week=Week.SunDay;

    }

    public GameTime(GameTime gameTime)
    {
        gameDate = new GameDate(gameTime.gameDate);
        hour = gameTime.hour;
        minute = gameTime.minute;
        week = gameTime.week;
    }

    public GameTime(int _year, Season _season, int _date, int _hour, int _minute)
    {
        gameDate=new GameDate();
        gameDate.year = _year;
        gameDate.season = _season;
        gameDate.date = _date;
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
            gameDate.date += hour / 24;
            hour = hour % 24;

           
            GameComponentData.gameData.charactorTitleAction.AddSleepDays(); 
            GameComponentData.gameData.plantAction.PlantGrowing();
            GameComponentData.gameData.pastureAction.DayUpdata();
            GameComponentData.gameData.gameManager.ChildDateCost();
            GameComponentData.gameData.gameManager.gamePlayer.isAnMo = false;
            GameComponentData.gameData.gameManager.gamePlayer.isMarriedFood = false;

            
        }
        if (gameDate.date > 30)
        {
            int seasonId = (int)gameDate.season;
            if (seasonId < 4)
            {
                seasonId++;
            }
            else if(seasonId == 4)
            {
                gameDate.year++;
                seasonId = 1;
            }
            gameDate.season = (Season)seasonId;
            gameDate.date = 1;
           
           
            GameComponentData.gameData.gameManager.UpdataMapSeason();
        }
        int x = gameDate.date %6;
        if (x == 0)
        {
            week=Week.FriDay;
        }
        if (x==1)
        {
            week=Week.SunDay;
        }
        if (x == 2)
        {
            week = Week.Monday;
        }
        if (x == 3)
        {
            week = Week.TuesDay;
        }
        if (x == 4)
        {
            week = Week.WednesDay;
        }
        if (x == 5)
        {
            week = Week.ThursDay;
        }

        if (LanguageManage.nowLanguage == SystemLanguage.Chinese)
        {
            InformationController.instance.AddInformation("*" + gameDate.year + "年" + gameDate.season + "之月" + gameDate.date + "日");
        }
        else
        {
            InformationController.instance.AddInformation("*" + gameDate.date + "," + LanguageManage.SwitchStr(gameDate.season.ToString()) + "," + gameDate.year + LanguageManage.SwitchStr("年"));
        }
    }
    public void Sleep()
    {
        minute = 0;
        hour = 6;
        gameDate.date++;
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
    public GameTime nowGameTime;
    public GameTime startTime;
    public float timeRunScale=1;

    

    public TimeDisplayAction timeDisplayAction;
    [HideInInspector]
    public List<GameDate> gameDates;

    public override void Init()
    {
        base.Init();
        nowGameTime = startTime;
        CreatData();
    }
    public string GameTimeToString()
    {
        string timeStr = nowGameTime.gameDate.year.ToString();
        timeStr += "," + nowGameTime.gameDate.season;
        timeStr += "," + nowGameTime.gameDate.date;
        return timeStr;
    }

    public GameDate StringToGameTime(string timeStr)
    {
        var x = timeStr.Split(',');
        GameDate gameDate=new GameDate();
        gameDate.year = int.Parse(x[0]);
        gameDate.season = (Season) Enum.Parse(typeof(Season),x[1]);
        gameDate.date = int.Parse(x[2]);
        return gameDate;
    }

    public int DaysToSave(string timeStr)
    {
        GameDate saveDate = StringToGameTime(timeStr);
        int years = nowGameTime.gameDate.year - saveDate.year;
        int months= nowGameTime.gameDate.season - saveDate.season;
        int days = nowGameTime.gameDate.date - saveDate.date;
        return (years * 4 + months) * 30 + days;

    }
     
   public void CreatData()
    {
        gameDates=new List<GameDate>();
        for (int i = 1; i <= 120; i++)
        {
            int seasonId = (i-1) / 30 + 1;
            int date = i - (seasonId-1) * 30;
            Season season = (Season) seasonId;
            List<FestivalData> festivals=GameComponentData.gameData.festivalManager.FestivalDatas.FindAll(f=>f.season==season&&
            f.date==date);
            List<int> festivalIds=new List<int>();
            foreach (var festivalData in festivals)
            {
                festivalIds.Add(festivalData.id);
            }
            GameDate gameDate=new GameDate(nowGameTime.gameDate.year,season,date,festivalIds);
            gameDates.Add(gameDate);

        }
        timeDisplayAction.UpdataTime();
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
        timeDisplayAction.UpdataTime();
        //StartTimeRun();
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
            timeDisplayAction.UpdataTime();
            yield return new WaitForSeconds(waitTime);
        }
    }
	 
}
