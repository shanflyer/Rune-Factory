using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

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
    private EnvironmentData overrideDayEnvironmentData, overrideNightEnvironmentData;
    private EnvironmentData overrideDawnEnvironmentData, overrideDuskEnvironmentData;
    private bool overrideEnvironment;

    private EnvironmentData DayEnvironmentData => overrideEnvironment ? overrideDayEnvironmentData : dayEnvironmentData;
    private EnvironmentData NightEnvironmentData => overrideEnvironment ? overrideNightEnvironmentData : nightEnvironmentData;
    private EnvironmentData DawnEnvironmentData => overrideEnvironment ? overrideDawnEnvironmentData : dawnEnvironmentData;
    private EnvironmentData DuskEnvironmentData => overrideEnvironment ? overrideDuskEnvironmentData : duskEnvironmentData;

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
    private float timeValue;

    public float TimeValue=>timeValue;
    public void SetTime(int hour, int minute)
    {
        if (hour > 0)
            this.hour = hour;
        if (minute > 0)
            this.minute = minute;
        TimeInit();
    }

    public async void SetMapOverrideEnvironment(string dayEnvironmentDataName,
        string duskEnvironmentDataName, string dawnEnvironmentDataName, string nightEnvironmentDataName)
    {
        overrideDawnEnvironmentData = await GameDataManager.instance.GetAsyncData<EnvironmentData>(dawnEnvironmentDataName);
        overrideDuskEnvironmentData = await GameDataManager.instance.GetAsyncData<EnvironmentData>(duskEnvironmentDataName);
        overrideDayEnvironmentData = await GameDataManager.instance.GetAsyncData<EnvironmentData>(dayEnvironmentDataName);
        overrideNightEnvironmentData = await GameDataManager.instance.GetAsyncData<EnvironmentData>(nightEnvironmentDataName);
        overrideEnvironment = true;
        SetLightValue();
    }

    public void ClearOverrideEnvironment()
    {
        overrideEnvironment = false;
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
        if (DawnEnvironmentData.GlobalColor == null)
            return;
        int nowMinute = hour * 60 + minute;

        timeValue = (nowMinute + mySecond * 0.05f) / (totalSunMinute + totalMoonMinute);
        EnvironmentManger.instance.UpDataMyLightTimeValue(timeValue);

        if (nowMinute >= dawnStart && nowMinute <= dawnEnd)
        {
            float lightValue = (float)(nowMinute - dawnStart + mySecond * 0.05f) / (60);
            var environmentLightData = new EnvironmentLightData();
            environmentLightData.cloudColor = DawnEnvironmentData.CloudColor.Evaluate(lightValue);
            environmentLightData.skyTopColor = DawnEnvironmentData.SkyTopColor.Evaluate(lightValue);
            environmentLightData.skyBottomColor = DawnEnvironmentData.SkyBottomColor.Evaluate(lightValue);
            environmentLightData.globalColor = DawnEnvironmentData.GlobalColor.Evaluate(lightValue);
            environmentLightData.globalIntensity = DawnEnvironmentData.GlobalIntensity.Evaluate(lightValue);
            environmentLightData.color = DawnEnvironmentData.Color.Evaluate(lightValue);
            environmentLightData.direction = new Vector3(DawnEnvironmentData.directionXValue.Evaluate(lightValue),
                          DawnEnvironmentData.directionYValue.Evaluate(lightValue), DawnEnvironmentData.directionZValue.Evaluate(lightValue));
            environmentLightData.intensity = DawnEnvironmentData.intensity.Evaluate(lightValue);
            environmentLightData.shadowValue = DawnEnvironmentData.shadowValue.Evaluate(lightValue);

            environmentLightData.skyHalfValue = DawnEnvironmentData.SkyHalfValue.Evaluate(lightValue);
            environmentLightData.skyTopColor = DawnEnvironmentData.SkyTopColor.Evaluate(lightValue);
            environmentLightData.skyBottomColor = DawnEnvironmentData.SkyBottomColor.Evaluate(lightValue);
            Color sunColor = DawnEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor *= DawnEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;
            environmentLightData.sunColor = sunColor;
            environmentLightData.sunScale = DawnEnvironmentData.sunScaleValue.Evaluate(lightValue);
            environmentLightData.sunPos = new Vector2(DawnEnvironmentData.sunXValue.Evaluate(lightValue),
                DawnEnvironmentData.sunYValue.Evaluate(lightValue));
            environmentLightData.cloudColor = DawnEnvironmentData.CloudColor.Evaluate(lightValue);
            environmentLightData.sunValue = 1;

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = environmentLightData
            };
            GameActionManager.instance.QueueAction(setEnvironmentLight, true);
        }
        else if (nowMinute > dawnEnd && nowMinute <= dayEnd)
        {
            night = false;
            float sunValue = (nowMinute - dayStart + mySecond * 0.05f) / (float)(totalSunMinute);

            Color sunColor = DayEnvironmentData.sunColor.Evaluate(sunValue);
            float a = sunColor.a;
            sunColor *= DayEnvironmentData.sunColorValue.Evaluate(sunValue);
            sunColor.a = a;
            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                {
                    globalColor = DayEnvironmentData.GlobalColor.Evaluate(sunValue),
                    globalIntensity = DayEnvironmentData.GlobalIntensity.Evaluate(sunValue),
                    color = DayEnvironmentData.Color.Evaluate(sunValue),
                    direction = new Vector3(DayEnvironmentData.directionXValue.Evaluate(sunValue),
                    DayEnvironmentData.directionYValue.Evaluate(sunValue), DayEnvironmentData.directionZValue.Evaluate(sunValue)),
                    intensity = DayEnvironmentData.intensity.Evaluate(sunValue),
                    shadowValue = DayEnvironmentData.shadowValue.Evaluate(sunValue),

                    skyHalfValue = DayEnvironmentData.SkyHalfValue.Evaluate(sunValue),
                    cloudColor = DayEnvironmentData.CloudColor.Evaluate(sunValue),
                    skyTopColor = DayEnvironmentData.SkyTopColor.Evaluate(sunValue),
                    skyBottomColor = DayEnvironmentData.SkyBottomColor.Evaluate(sunValue),
                    sunColor = sunColor,
                    sunScale = DayEnvironmentData.sunScaleValue.Evaluate(sunValue),
                    sunPos = new Vector2(DayEnvironmentData.sunXValue.Evaluate(sunValue),
                      DayEnvironmentData.sunYValue.Evaluate(sunValue)),
                    sunValue = 1
                }
            };
            GameActionManager.instance.QueueAction(setEnvironmentLight, true);
        }
        else if (nowMinute > dayEnd && nowMinute < duskEnd)
        {
            float lightValue = (float)(nowMinute - duskStart + mySecond * 0.05f) / (60);

            Color sunColor = DuskEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor *= DuskEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                {
                    globalColor = DuskEnvironmentData.GlobalColor.Evaluate(lightValue),
                    globalIntensity = DuskEnvironmentData.GlobalIntensity.Evaluate(lightValue),
                    color = DuskEnvironmentData.Color.Evaluate(lightValue),
                    direction = new Vector3(DuskEnvironmentData.directionXValue.Evaluate(lightValue),
                               DuskEnvironmentData.directionYValue.Evaluate(lightValue), DuskEnvironmentData.directionZValue.Evaluate(lightValue)),
                    intensity = DuskEnvironmentData.intensity.Evaluate(lightValue),
                    shadowValue = DuskEnvironmentData.shadowValue.Evaluate(lightValue),

                    skyHalfValue = DuskEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                    cloudColor = DuskEnvironmentData.CloudColor.Evaluate(lightValue),
                    skyTopColor = DuskEnvironmentData.SkyTopColor.Evaluate(lightValue),
                    skyBottomColor = DuskEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                    sunColor = sunColor,
                    sunScale = DuskEnvironmentData.sunScaleValue.Evaluate(lightValue),
                    sunPos = new Vector2(DuskEnvironmentData.sunXValue.Evaluate(lightValue),
                      DuskEnvironmentData.sunYValue.Evaluate(lightValue)),
                    sunValue = 1
                }
            };
            GameActionManager.instance.QueueAction(setEnvironmentLight, true);
        }
        else if (nowMinute > dawnStart)
        {
            night = true;
            float lightValue = (float)(nowMinute - duskEnd + mySecond * 0.05f) / totalMoonMinute;

            Color sunColor = NightEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor *= NightEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                {
                    globalColor = NightEnvironmentData.GlobalColor.Evaluate(lightValue),
                    globalIntensity = NightEnvironmentData.GlobalIntensity.Evaluate(lightValue),
                    color = NightEnvironmentData.Color.Evaluate(lightValue),
                    direction = new Vector3(NightEnvironmentData.directionXValue.Evaluate(lightValue),
                               NightEnvironmentData.directionYValue.Evaluate(lightValue), NightEnvironmentData.directionZValue.Evaluate(lightValue)),
                    intensity = NightEnvironmentData.intensity.Evaluate(lightValue),
                    shadowValue = NightEnvironmentData.shadowValue.Evaluate(lightValue),

                    skyHalfValue = NightEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                    cloudColor = NightEnvironmentData.CloudColor.Evaluate(lightValue),
                    skyTopColor = NightEnvironmentData.SkyTopColor.Evaluate(lightValue),
                    skyBottomColor = NightEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                    sunColor = sunColor,
                    sunScale = NightEnvironmentData.sunScaleValue.Evaluate(lightValue),
                    sunPos = new Vector2(NightEnvironmentData.sunXValue.Evaluate(lightValue),
                      NightEnvironmentData.sunYValue.Evaluate(lightValue)),
                    sunValue = 0
                }
            };
            GameActionManager.instance.QueueAction(setEnvironmentLight, true);
        }
        else
        {
            night = true;
            float lightValue = (float)(totalMoonMinute - (dawnStart - nowMinute) + mySecond * 0.05f) / totalMoonMinute;

            Color sunColor = NightEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor *= NightEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                {
                    globalColor = NightEnvironmentData.GlobalColor.Evaluate(lightValue),
                    globalIntensity = NightEnvironmentData.GlobalIntensity.Evaluate(lightValue),
                    color = NightEnvironmentData.Color.Evaluate(lightValue),
                    direction = new Vector3(NightEnvironmentData.directionXValue.Evaluate(lightValue),
                               NightEnvironmentData.directionYValue.Evaluate(lightValue), NightEnvironmentData.directionZValue.Evaluate(lightValue)),
                    intensity = NightEnvironmentData.intensity.Evaluate(lightValue),
                    shadowValue = NightEnvironmentData.shadowValue.Evaluate(lightValue),

                    skyHalfValue = NightEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                    cloudColor = NightEnvironmentData.CloudColor.Evaluate(lightValue),
                    skyTopColor = NightEnvironmentData.SkyTopColor.Evaluate(lightValue),
                    skyBottomColor = NightEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                    sunColor = sunColor,
                    sunScale = NightEnvironmentData.sunScaleValue.Evaluate(lightValue),
                    sunPos = new Vector2(NightEnvironmentData.sunXValue.Evaluate(lightValue),
                      NightEnvironmentData.sunYValue.Evaluate(lightValue)),
                    sunValue = 0
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
            mySecond = mySecond % 20;
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
                float moonOffSet = (30 - date) / 15.0f;
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

    public void SetTime(int hour = -1, int minute = -1)
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
        GameActionManager.instance.AddListener<SetMapOverrideEnvironment>(SetMapOverrideEnvironment);
        GameActionManager.instance.AddListener<ClearOverrideEnvironment>(ClearOverrideEnvironment);
        GameActionManager.instance.AddListener<PlayerSleep>(PlayerSleep);
        // CreatData();
    }
    void PlayerSleep(PlayerSleep playerSleep)
    {

    }

    private void ClearOverrideEnvironment(ClearOverrideEnvironment clearOverrideEnvironment)
    {
        nowGameTime.ClearOverrideEnvironment();
    }

    private void SetMapOverrideEnvironment(SetMapOverrideEnvironment setMapOverrideEnvironment)
    {
        nowGameTime.SetMapOverrideEnvironment(setMapOverrideEnvironment.dayEnvironmentDataName,
            setMapOverrideEnvironment.duskEnvironmentDataName, setMapOverrideEnvironment.dawnEnvironmentDataName,
            setMapOverrideEnvironment.nightEnvironmentDataName);
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
        if (TimeRunIEnumerator != null && GameObjectCurveController.instance.UpDataComponent)
        {
            GameObjectCurveController.instance.UpDataComponent.StopCoroutine(TimeRunIEnumerator);
        }
    }

    public void NextDate()
    {
        nowGameTime.minute = 0;
        nowGameTime.AddDate();
    }

    private void LerpGameTime(LerpGameTime lerpGameTime)
    {
        StopTimeRun();
        var lerpTimeIEnumerator = LerpTime(lerpGameTime.targetHour, lerpGameTime.targetMinute, lerpGameTime.totalTime);
        GameObjectCurveController.instance.UpDataComponent.StartCoroutine(lerpTimeIEnumerator);
    }

    private IEnumerator LerpTime(int targetHour, int targetMinue, float totalTime)
    {
        int startValue = (Hour * 60 + Minute) * 20;
        int endValue = (targetHour * 60 + targetMinue) * 20;
        int addValue = (int)math.round((endValue - startValue) / totalTime * UnityEngine.Time.fixedDeltaTime);
        float timeValue = 0;
        while (timeValue < totalTime)
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