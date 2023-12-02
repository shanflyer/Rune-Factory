using BehaviorDesigner.Runtime.Tasks.Unity.UnityLight;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public enum Week
{
    SunDay = 0,
    Monday = 1,
    TuesDay = 2,
    WednesDay = 3,
    ThursDay = 4,
    FriDay = 5
}

[System.Serializable]
public enum Season
{
    Default = 0, 春 = 1, 夏 = 2, 秋 = 3, 冬 = 4
}

[System.Serializable]
public struct GameDate : IReferenceData, INativeData
{
    public Season season;
    public int date;
    public UnsafeList<int> FestivaList;
    public UnsafeList<int> CustomFestival;

    public GameDate(Season _season, int _date, List<int> _festivals)
    {
        season = _season;
        date = _date;
        FestivaList = new UnsafeList<int>(4, Allocator.TempJob);
        foreach (var festival in _festivals)
        {
            FestivaList.Add(festival);
        }
        CustomFestival = new UnsafeList<int>(4, Allocator.TempJob);
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

    public Season Season
    {
        get
        {
            return season;
        }
        set
        {
            season = value; InitSeasonData();
        }
    }

    private Season season;
    public int date;
    public int hour;
    public int minute;
    public int mySecond;
    public Week week;

    private SeasonData SeasonData;
    private EnvironmentData dayEnvironmentData, nightEnvironmentData;
    private EnvironmentData dawnEnvironmentData, duskEnvironmentData;

    /// <summary>
    /// 白天时间
    /// </summary>
    private int dayStart, dayEnd;

    /// <summary>
    /// 黎明时间
    /// </summary>
    private int dawnStart, dawnEnd;

    /// <summary>
    /// 黄昏时间
    /// </summary>
    private int duskStart, duskEnd;

    private int totalSunMinute, totalMoonMinute;
    private bool night = false;

    public void SetTime(int hour,int minute)
    {
        if(hour>0)
            this.hour = hour;
        if (minute > 0)
            this.minute = minute;
        TimeInit();
    }
    private async void InitSeasonData()
    {
        SeasonData = await GameDataManager.instance.GetAsyncData<SeasonData>(season.ToString());
        dayStart = SeasonData.sunupHour * 60 + SeasonData.sunupMinute + 30;
        dayEnd = SeasonData.sundownHour * 60 + SeasonData.sundownMinute - 30;

        dawnStart = dayStart - 60;
        dawnEnd = dayStart;

        duskStart = dayEnd;
        duskEnd = dayEnd + 60;

        totalSunMinute = dayEnd - dayStart;
        totalMoonMinute = 22 * 60 - dayEnd + dayStart;

        dawnEnvironmentData = await GameDataManager.instance.GetAsyncData<EnvironmentData>(SeasonData.dawnEnvironmentDataName);
        duskEnvironmentData = await GameDataManager.instance.GetAsyncData<EnvironmentData>(SeasonData.duskEnvironmentDataName);
        dayEnvironmentData = await GameDataManager.instance.GetAsyncData<EnvironmentData>(SeasonData.dayEnvironmentDataName);
        nightEnvironmentData = await GameDataManager.instance.GetAsyncData<EnvironmentData>(SeasonData.nightEnvironmentDataName);
        SetLightValue();
    }

    private void SetLightValue()
    {
        if (dawnEnvironmentData.GlobalColor == null)
            return;
        int nowMinute = hour * 60 + minute;

        if (nowMinute >= dawnStart && nowMinute <= dawnEnd)
        {
            float lightValue = (float)(nowMinute - dawnStart+mySecond* 0.05f) / (60);
            var environmentLightData = new EnvironmentLightData();
            environmentLightData.cloudColor = dawnEnvironmentData.CloudColor.Evaluate(lightValue);
            environmentLightData.skyTopColor = dawnEnvironmentData.SkyTopColor.Evaluate(lightValue);
            environmentLightData.skyBottomColor = dawnEnvironmentData.SkyBottomColor.Evaluate(lightValue);
            environmentLightData.globalColor = dawnEnvironmentData.GlobalColor.Evaluate(lightValue);
            environmentLightData.globalIntensity = dawnEnvironmentData.GlobalIntensity.Evaluate(lightValue);
            environmentLightData.color = dawnEnvironmentData.Color.Evaluate(lightValue);
            environmentLightData.direction = new Vector3(dawnEnvironmentData.directionXValue.Evaluate(lightValue),
                          dawnEnvironmentData.directionYValue.Evaluate(lightValue), 120);
            environmentLightData.intensity = dawnEnvironmentData.intensity.Evaluate(lightValue);
            environmentLightData.shadowValue = dawnEnvironmentData.shadowValue.Evaluate(lightValue);

            environmentLightData.skyHalfValue = dawnEnvironmentData.SkyHalfValue.Evaluate(lightValue);
            environmentLightData.skyTopColor = dawnEnvironmentData.SkyTopColor.Evaluate(lightValue);
            environmentLightData.skyBottomColor = dawnEnvironmentData.SkyBottomColor.Evaluate(lightValue);
            Color sunColor = dawnEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor*=dawnEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;
            environmentLightData.sunColor = sunColor;
            environmentLightData.sunScale = dawnEnvironmentData.sunScaleValue.Evaluate(lightValue);
            environmentLightData.sunPos = new Vector2(dawnEnvironmentData.sunXValue.Evaluate(lightValue),
                dawnEnvironmentData.sunYValue.Evaluate(lightValue));
            environmentLightData.cloudColor = dawnEnvironmentData.CloudColor.Evaluate(lightValue);
            environmentLightData.sunValue = 1;

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = environmentLightData
            };
            GameActionManager.instance.QueueAction(setEnvironmentLight, true);
        }
        else if (nowMinute> dawnEnd&&nowMinute <= dayEnd)
        {
            night = false;
            float sunValue = (nowMinute - dayStart + mySecond * 0.05f) / (float)(totalSunMinute);

            Color sunColor = dayEnvironmentData.sunColor.Evaluate(sunValue); 
            float a = sunColor.a;
            sunColor *= dayEnvironmentData.sunColorValue.Evaluate(sunValue);
            sunColor.a = a;
            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                { 
                    globalColor = dayEnvironmentData.GlobalColor.Evaluate(sunValue),
                    globalIntensity = dayEnvironmentData.GlobalIntensity.Evaluate(sunValue),
                    color = dayEnvironmentData.Color.Evaluate(sunValue),
                    direction = new Vector3(dayEnvironmentData.directionXValue.Evaluate(sunValue),
                    dayEnvironmentData.directionYValue.Evaluate(sunValue), 120),
                    intensity = dayEnvironmentData.intensity.Evaluate(sunValue),
                    shadowValue = dayEnvironmentData.shadowValue.Evaluate(sunValue),

                    skyHalfValue = dayEnvironmentData.SkyHalfValue.Evaluate(sunValue), 
                    cloudColor = dayEnvironmentData.CloudColor.Evaluate(sunValue),
                    skyTopColor = dayEnvironmentData.SkyTopColor.Evaluate(sunValue),
                    skyBottomColor = dayEnvironmentData.SkyBottomColor.Evaluate(sunValue),
                    sunColor = sunColor,
                    sunScale = dayEnvironmentData.sunScaleValue.Evaluate(sunValue),
                    sunPos = new Vector2(dayEnvironmentData.sunXValue.Evaluate(sunValue),
                      dayEnvironmentData.sunYValue.Evaluate(sunValue)), 
                    sunValue=1
                }
            };
            GameActionManager.instance.QueueAction(setEnvironmentLight, true);
        }
        else if (nowMinute > dayEnd && nowMinute < duskEnd)
        {
            float lightValue = (float)(nowMinute - duskStart + mySecond * 0.05f) / (60);

            Color sunColor = duskEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor *= duskEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                { 
                    globalColor = duskEnvironmentData.GlobalColor.Evaluate(lightValue),
                    globalIntensity = duskEnvironmentData.GlobalIntensity.Evaluate(lightValue),
                    color = duskEnvironmentData.Color.Evaluate(lightValue),
                    direction = new Vector3(duskEnvironmentData.directionXValue.Evaluate(lightValue),
                               duskEnvironmentData.directionYValue.Evaluate(lightValue), 120),
                    intensity = duskEnvironmentData.intensity.Evaluate(lightValue),
                    shadowValue = duskEnvironmentData.shadowValue.Evaluate(lightValue),

                    skyHalfValue = duskEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                    cloudColor = duskEnvironmentData.CloudColor.Evaluate(lightValue),
                    skyTopColor = duskEnvironmentData.SkyTopColor.Evaluate(lightValue),
                    skyBottomColor = duskEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                    sunColor = sunColor,
                    sunScale = duskEnvironmentData.sunScaleValue.Evaluate(lightValue),
                    sunPos = new Vector2(duskEnvironmentData.sunXValue.Evaluate(lightValue),
                      duskEnvironmentData.sunYValue.Evaluate(lightValue)),
                    sunValue=1
                }
            };
            GameActionManager.instance.QueueAction(setEnvironmentLight, true);
        }
        else if (nowMinute > dawnStart)
        {
            night = true;
            float lightValue = (float)(nowMinute - duskEnd + mySecond * 0.05f) / totalMoonMinute;

            Color sunColor = nightEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor *= nightEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                { 
                    globalColor = nightEnvironmentData.GlobalColor.Evaluate(lightValue),
                    globalIntensity = nightEnvironmentData.GlobalIntensity.Evaluate(lightValue),
                    color = nightEnvironmentData.Color.Evaluate(lightValue),
                    direction = new Vector3(nightEnvironmentData.directionXValue.Evaluate(lightValue),
                               nightEnvironmentData.directionYValue.Evaluate(lightValue), 120),
                    intensity = nightEnvironmentData.intensity.Evaluate(lightValue),
                    shadowValue = nightEnvironmentData.shadowValue.Evaluate(lightValue),

                    skyHalfValue = nightEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                    cloudColor = nightEnvironmentData.CloudColor.Evaluate(lightValue),
                    skyTopColor = nightEnvironmentData.SkyTopColor.Evaluate(lightValue),
                    skyBottomColor = nightEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                    sunColor = sunColor,
                    sunScale = nightEnvironmentData.sunScaleValue.Evaluate(lightValue),
                    sunPos = new Vector2(nightEnvironmentData.sunXValue.Evaluate(lightValue),
                      nightEnvironmentData.sunYValue.Evaluate(lightValue)),
                    sunValue=0
                }
            };
            GameActionManager.instance.QueueAction(setEnvironmentLight, true);
        }
        else
        {
            night = true;
            float lightValue = (float)(totalMoonMinute - (dawnStart - nowMinute) + mySecond * 0.05f) / totalMoonMinute;

            Color sunColor = nightEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor *= nightEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                { 
                    globalColor = nightEnvironmentData.GlobalColor.Evaluate(lightValue),
                    globalIntensity = nightEnvironmentData.GlobalIntensity.Evaluate(lightValue),
                    color = nightEnvironmentData.Color.Evaluate(lightValue),
                    direction = new Vector3(nightEnvironmentData.directionXValue.Evaluate(lightValue),
                               nightEnvironmentData.directionYValue.Evaluate(lightValue), 120),
                    intensity = nightEnvironmentData.intensity.Evaluate(lightValue),
                    shadowValue = nightEnvironmentData.shadowValue.Evaluate(lightValue),

                    skyHalfValue = nightEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                    cloudColor = nightEnvironmentData.CloudColor.Evaluate(lightValue),
                    skyTopColor = nightEnvironmentData.SkyTopColor.Evaluate(lightValue),
                    skyBottomColor = nightEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                    sunColor = sunColor,
                    sunScale = nightEnvironmentData.sunScaleValue.Evaluate(lightValue),
                    sunPos = new Vector2(nightEnvironmentData.sunXValue.Evaluate(lightValue),
                      nightEnvironmentData.sunYValue.Evaluate(lightValue)),
                    sunValue=0
                }
            };
            GameActionManager.instance.QueueAction(setEnvironmentLight, true);
        }
    }

    public GameTime()
    {
        hour = 0;
        minute = 0;
        week = Week.SunDay;
    }

    public int GetTimeKey()
    {
        return (year * 1000 + (int)Season * 100) + date;
    }

    public int GetTimeKeyNoYear()
    {
        return ((int)Season * 100) + date;
    }

    public GameTime(int _year, Season _season, int _date, int _hour, int _minute)
    {
        year = _year;
        Season = _season;
        date = _date;
        hour = _hour;
        minute = _minute;
        TimeInit();
    }

    public void TimeRun()
    {
#if UNITY_EDITOR
        if (!GameTimeManager.instance.runTime)
        {
            return;
        }
#endif 
        mySecond++;
        TimeInit();
    }

    public void AddDate()
    {
        hour = 30;
        TimeInit();
    }

    public void TimeInit()
    {
        if (mySecond >= 20)
        {
            mySecond = mySecond%20;
            minute++;
        }
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
            if (date <= 15)
            {
                float moonOffSet = date / 15.0f;
                Shader.SetGlobalFloat("_moonOffSet", moonOffSet);
            }
            else
            {
                float moonOffSet = (30-date) / 15.0f;
                Shader.SetGlobalFloat("_moonOffSet", moonOffSet);
            }
        }
        if (date > 30)
        {
            int seasonId = (int)Season;
            if (seasonId < 4)
            {
                seasonId++;
            }
            else if (seasonId == 4)
            {
                year++;
                seasonId = 1;
            }
            Season = (Season)seasonId;
            date = 1;

            if (LanguageManage.nowLanguage == SystemLanguage.Chinese)
            {
                InformationController.instance.AddInformation("*" + year + "年" + Season + "之月" + date + "日");
            }
            else
            {
                InformationController.instance.AddInformation("*" + date + "," + LanguageManage.SwitchStr(Season.ToString()) + "," + year + LanguageManage.SwitchStr("年"));
            }
        }
        int x = date % 6;
        week = (Week)x; 
        SetLightValue();
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
    public float timeRunScale = 1;
#if UNITY_EDITOR
    public bool runTime = true;
#endif
    public string NowGameTime => LanguageManage.instance.GameTimeToString(nowGameTime);
    public int Year => nowGameTime.year;
    public Season Season => nowGameTime.Season;
    public int Day => nowGameTime.date;

    public int Hour
    {
        get
        {
            if (nowGameTime != null)
            {
                return nowGameTime.hour;
            }
            return 0;
        }
    }
    public int Minute
    {
        get
        {
            if (nowGameTime != null)
            {
                return nowGameTime.minute;
            }
            return 0;
        }
    }
    public void SetTime(int hour=-1,int minute=-1)
    {
        if (nowGameTime != null)
        {
            nowGameTime.SetTime(hour, minute);
        }
    }

    private MyNativeData<GameDate> gameDates = new MyNativeData<GameDate>();

    public List<GameDate> GetGameDataForSeason(Season season)
    {
        List<GameDate> results = new List<GameDate>();
        for (int i = 1; i <= 30; i++)
        {
            int key = (int)season * 100 + i;
            if (gameDates.GetData(key, out var gameDate))
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
        GameActionManager.instance.AddListener<LerpGameTime>(LerpGameTime);
       // CreatData();
    }

    public void ZeroGameTime()
    {
        if (nowGameTime == null)
        {
            nowGameTime = new GameTime
            {
                Season = Season.春,
                hour = 12
            };
            nowGameTime.SetTime(12, 0);
           // StartTimeRun();
        } 
    }

    public string GameTimeToString()
    {
        string timeStr = nowGameTime.year.ToString();
        timeStr += "," + nowGameTime.Season;
        timeStr += "," + nowGameTime.date;
        return timeStr;
    }

    public void StringToGameTime(string timeStr)
    {
        var x = timeStr.Split(',');
        nowGameTime.year = int.Parse(x[0]);
        nowGameTime.Season = (Season)Enum.Parse(typeof(Season), x[1]);
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
        if (FestivalManager.instance.FestivalDatas == null)
        {
            return;
        }
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

    private IEnumerator TimeRunIEnumerator;

    public void StartTimeRun()
    {
        if (GameObjectCurveController.instance.UpDataComponent)
        {
            TimeRunIEnumerator = TimeRun();
            GameObjectCurveController.instance.UpDataComponent.StartCoroutine(TimeRunIEnumerator);
        }
       
    }

    public void StopTimeRun()
    {
        if (TimeRunIEnumerator != null&& GameObjectCurveController.instance.UpDataComponent)
        {
            GameObjectCurveController.instance.UpDataComponent.StopCoroutine(TimeRunIEnumerator);
        }
    }

    public void NextDate()
    {
        nowGameTime.minute = 0;
        nowGameTime.AddDate();
    }

    void LerpGameTime(LerpGameTime lerpGameTime)
    {
        StopTimeRun();
        var lerpTimeIEnumerator = LerpTime(lerpGameTime.targetHour, lerpGameTime.targetMinute, lerpGameTime.totalTime);
        GameObjectCurveController.instance.UpDataComponent.StartCoroutine(lerpTimeIEnumerator);
    }
    private IEnumerator LerpTime(int targetHour,int targetMinue,float totalTime)
    {
        int startValue = (Hour * 60 + Minute) * 20;
        int endValue= (targetHour * 60 + targetMinue) * 20;
        int addValue =(int)math.round((endValue - startValue) / totalTime * UnityEngine.Time.fixedDeltaTime);
        float timeValue = 0;
        while (timeValue<totalTime)
        {
            timeValue += UnityEngine.Time.deltaTime;
            nowGameTime.mySecond += addValue;
            nowGameTime.TimeInit();
            //Debug.Log($"Time:{timeValue}-hour:{nowGameTime.hour}-minute:{nowGameTime.minute}--second:{nowGameTime.mySecond}");

            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator TimeRun()
    {
        while (true)
        {
            float waitTime = 0.05f;
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