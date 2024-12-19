using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
public class GameDate : IReferenceData, INativeData
{
    public Season season;
    public int date;
    public List<int> FestivaList;
    public List<int> CustomFestival;

    public GameDate(Season _season, int _date, List<int> _festivals)
    {
        season = _season;
        date = _date;
        FestivaList = new List<int>(4);
        foreach (var festival in _festivals)
        {
            FestivaList.Add(festival);
        }
        CustomFestival = new List<int>(4);
    }

    public void Dispose()
    {
    }

    public int Key => (int)season * 100 + date;
}



public class GameTimeManager : Singleton<GameTimeManager>
{
    [System.Serializable]
    internal class GameTime
    {
        public int year = 1;

        public Season Season
        {
            get
            {
                return season;
            }
            set
            {
                season = value; 
                InitSeasonData();
            }
        }

        private Season season;
        private float seasonValue;
        private float dayValue;

        public int day 
        {
            get => _day;
            set
            {
                _day = value;
                dayValue = (((int)season-1) * 30+_day)/120.0f;
            }
        }
        private int _day = 1;
        public int hour
        {
            get => _hour;
            set
            {
                if (_hour != value)
                {
                    _hour = value;
                    if (_hour == 6)
                    {
                        CreateWeather();
                    }
                    WeatherManager.instance.RefreshWeather(value);
                    GameActionManager.instance.QueueAction(new NewHour());
                }
            }
        }
        private int _hour;
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
        public bool night { get; private set; }
        /// <summary>
        /// 黄昏时间
        /// </summary>
        private int duskStart, duskEnd;

        private int totalSunMinute, totalMoonMinute;
        private int totalMinute = 1440;// 24 * 60;
                                       //private bool night = false;
        private float timeValue;

        public float TimeValue => timeValue;

        public void SetDate(int day)
        {
            if (day <= 15)
            {
                float moonOffSet = day / 15.0f;
                Shader.SetGlobalFloat("_moonOffSet", moonOffSet);
            }
            else
            {
                float moonOffSet = (30 - day) / 15.0f;
                Shader.SetGlobalFloat("_moonOffSet", moonOffSet);
            }
            this.day = day;
            TimeInit();
            UpDataGameTimeAction();
        }
        public void SetTime(int hour, int minute)
        {
            if (this.hour != hour)
            {
                GameActionManager.instance.QueueAction(newHour);
            }
            if (hour > 0)
                this.hour = hour;
            if (minute > 0)
                this.minute = minute;
             
            TimeInit();
            UpDataGameTimeAction();
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
            if (waitCreatWeather)
            {
                CreateWeather();
            }
        }

        private void SetLightValue()
        {
            if (DawnEnvironmentData.GlobalColor == null)
                return;
            int nowMinute = hour * 60 + minute;

            timeValue = (nowMinute + mySecond * 0.05f) / totalMinute;
            EnvironmentManger.instance.UpDataMyLightTimeValue(timeValue);

            if (nowMinute >= dawnStart && nowMinute <= dawnEnd)
            {
                float lightValue = (float)(nowMinute - dawnStart + mySecond * 0.05f) / (60);
                var environmentLightData = new EnvironmentLightData();
                environmentLightData.cloudColor = DawnEnvironmentData.CloudColor.Evaluate(lightValue);
                environmentLightData.skyTopColor = DawnEnvironmentData.SkyTopColor.Evaluate(lightValue);
                environmentLightData.skyBottomColor = DawnEnvironmentData.SkyBottomColor.Evaluate(lightValue);
                environmentLightData.globalColor = DawnEnvironmentData.GlobalColor.Evaluate(lightValue); 
                environmentLightData.color = DawnEnvironmentData.Color.Evaluate(lightValue);
                environmentLightData.direction = new Vector3(DawnEnvironmentData.directionXValue.Evaluate(lightValue),
                              DawnEnvironmentData.directionYValue.Evaluate(lightValue), 0); 
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
                environmentLightData.flareColor = DawnEnvironmentData.flareColor.Evaluate(lightValue);

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
                        color = DayEnvironmentData.Color.Evaluate(sunValue),
                        direction = new Vector3(DayEnvironmentData.directionXValue.Evaluate(sunValue),
                        DayEnvironmentData.directionYValue.Evaluate(sunValue), 0),
                        shadowValue = DayEnvironmentData.shadowValue.Evaluate(sunValue),

                        skyHalfValue = DayEnvironmentData.SkyHalfValue.Evaluate(sunValue),
                        cloudColor = DayEnvironmentData.CloudColor.Evaluate(sunValue),
                        skyTopColor = DayEnvironmentData.SkyTopColor.Evaluate(sunValue),
                        skyBottomColor = DayEnvironmentData.SkyBottomColor.Evaluate(sunValue),
                        sunColor = sunColor,
                        sunScale = DayEnvironmentData.sunScaleValue.Evaluate(sunValue),
                        sunPos = new Vector2(DayEnvironmentData.sunXValue.Evaluate(sunValue),
                          DayEnvironmentData.sunYValue.Evaluate(sunValue)),
                        sunValue = 1,
                        flareColor = DayEnvironmentData.flareColor.Evaluate(sunValue)
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
                        color = DuskEnvironmentData.Color.Evaluate(lightValue),
                        direction = new Vector3(DuskEnvironmentData.directionXValue.Evaluate(lightValue),
                                   DuskEnvironmentData.directionYValue.Evaluate(lightValue), 0), 
                        shadowValue = DuskEnvironmentData.shadowValue.Evaluate(lightValue),

                        skyHalfValue = DuskEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                        cloudColor = DuskEnvironmentData.CloudColor.Evaluate(lightValue),
                        skyTopColor = DuskEnvironmentData.SkyTopColor.Evaluate(lightValue),
                        skyBottomColor = DuskEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                        sunColor = sunColor,
                        sunScale = DuskEnvironmentData.sunScaleValue.Evaluate(lightValue),
                        sunPos = new Vector2(DuskEnvironmentData.sunXValue.Evaluate(lightValue),
                          DuskEnvironmentData.sunYValue.Evaluate(lightValue)),
                        sunValue = 1,
                        flareColor = DuskEnvironmentData.flareColor.Evaluate(lightValue)
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
                        color = NightEnvironmentData.Color.Evaluate(lightValue),
                        direction = new Vector3(NightEnvironmentData.directionXValue.Evaluate(lightValue),
                                   NightEnvironmentData.directionYValue.Evaluate(lightValue), 0),
                        shadowValue = NightEnvironmentData.shadowValue.Evaluate(lightValue),

                        skyHalfValue = NightEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                        cloudColor = NightEnvironmentData.CloudColor.Evaluate(lightValue),
                        skyTopColor = NightEnvironmentData.SkyTopColor.Evaluate(lightValue),
                        skyBottomColor = NightEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                        sunColor = sunColor,
                        sunScale = NightEnvironmentData.sunScaleValue.Evaluate(lightValue),
                        sunPos = new Vector2(NightEnvironmentData.sunXValue.Evaluate(lightValue),
                          NightEnvironmentData.sunYValue.Evaluate(lightValue)),
                        sunValue = 0,
                        flareColor = NightEnvironmentData.flareColor.Evaluate(lightValue)
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
                        color = NightEnvironmentData.Color.Evaluate(lightValue),
                        direction = new Vector3(NightEnvironmentData.directionXValue.Evaluate(lightValue),
                                   NightEnvironmentData.directionYValue.Evaluate(lightValue), 0),
                        shadowValue = NightEnvironmentData.shadowValue.Evaluate(lightValue),

                        skyHalfValue = NightEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                        cloudColor = NightEnvironmentData.CloudColor.Evaluate(lightValue),
                        skyTopColor = NightEnvironmentData.SkyTopColor.Evaluate(lightValue),
                        skyBottomColor = NightEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                        sunColor = sunColor,
                        sunScale = NightEnvironmentData.sunScaleValue.Evaluate(lightValue),
                        sunPos = new Vector2(NightEnvironmentData.sunXValue.Evaluate(lightValue),
                          NightEnvironmentData.sunYValue.Evaluate(lightValue)),
                        sunValue = 0,
                        flareColor = NightEnvironmentData.flareColor.Evaluate(lightValue)
                    }
                };
                GameActionManager.instance.QueueAction(setEnvironmentLight, true);
            }
        }

        bool waitCreatWeather = false;
        async void CreateWeather()
        {
            if (SeasonData.season == Season.Default)
            {
                waitCreatWeather = true;
                return;
            }
            else
            {
                waitCreatWeather = false;
            }
            CreatWeather creatWeather = new CreatWeather();
            if (GameDataSaveManager.instance.UserGameSaveData.nowWeathers.Count==0)
            {
                creatWeather.nowWeathers = await SetSeasonWeather(true);
            }
            creatWeather.nextWeather= await SetSeasonWeather(false);

            GameActionManager.instance.QueueAction(creatWeather);

            async Task<List<int>> SetSeasonWeather(bool nowDay = true)
            {
                int hour = 6;
                List<int> weaterDatas = new List<int>();
                for (int i = 0; i < 8; i++)
                {
                    float timeValue = hour * 60 / totalMinute;
                    float seasonValue = 0;

                    if (!nowDay)
                    {
                        seasonValue = dayValue * 4;
                    }
                    else
                    {
                        seasonValue = dayValue + 0.0083f;
                    }
                    GrowModelData growModelData = await GameDataManager.instance.GetAsyncData<GrowModelData>(SeasonData.WeatherCurveId);
                    float timeValue0 = seasonValue - (int)seasonValue;
                    float value0 = growModelData.curve.Evaluate(timeValue0);

                    GrowModelData growModelData1 = await GameDataManager.instance.GetAsyncData<GrowModelData>(SeasonData.WeatherCurveId1);
                    float value1 = growModelData1.curve.Evaluate(timeValue);

                    int randomId = (int)value0 * 10 + (int)value1;
                    int weatherId = GameRandom.instance.GetSingleRandomValue(randomId);

                    weaterDatas.Add(weatherId);
                }
                return weaterDatas;
            }
        }
         

        public GameTime()
        {
            hour = 0;
            minute = 0;
            week = Week.SunDay;
            newDay = new NewDay();
            newHour = new NewHour();
        }

        public int GetTimeKey()
        {
            return (year * 1000 + (int)Season * 100) + day;
        }

        public int GetTimeKeyNoYear()
        {
            return ((int)Season * 100) + day;
        }

        public GameTime(int _year, Season _season, int _day, int _hour, int _minute)
        {
            year = _year;
            Season = _season;
            day = _day;
            hour = _hour;
            minute = _minute;
            TimeInit();
            UpDataGameTimeAction();
        }

        public void TimeRun()
        {
#if UNITY_EDITOR
            if (!GameTimeManager.instance.runTime)
            {
                return;
            }
#endif
            mySecond += 1;
            TimeInit();
            if (minuteRefresh)
            {
                UpDataGameTimeAction();
            }
        }

        private NewDay newDay;
        private NewHour newHour;

        private bool minuteRefresh = false;

        public void TimeInit()
        {
            bool dayRefresh = false;
            if (mySecond >= 20)
            {
                minuteRefresh = true;
                minute += mySecond / 20;
                mySecond = mySecond % 20;
            }

            int oldhour = hour;

            if (minute >= 60)
            {
                hour += minute / 60;
                minute = minute % 20;
            }
            if (hour >= 24)
            {
                dayRefresh = true;
                day += hour / 24;
                hour = hour % 24;

                if (day <= 15)
                {
                    float moonOffSet = day / 15.0f;
                    Shader.SetGlobalFloat("_moonOffSet", moonOffSet);
                }
                else
                {
                    float moonOffSet = (30 - day) / 15.0f;
                    Shader.SetGlobalFloat("_moonOffSet", moonOffSet);
                }
            }
            if (day > 30)
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
                day = 1;
                float moonOffSet = day / 15.0f;
                Shader.SetGlobalFloat("_moonOffSet", moonOffSet);

                if (LanguageManage.nowLanguage == SystemLanguage.Chinese)
                {
                    InformationController.instance.AddInformation("*" + year + "年" + Season + "之月" + day + "日");
                }
                else
                {
                    InformationController.instance.AddInformation("*" + day + "," + LanguageManage.SwitchStr(Season.ToString()) + "," + year + LanguageManage.SwitchStr("年"));
                }
            }
            int x = day % 6;
            week = (Week)x;
            SetLightValue();

            if (oldhour != hour)
            {
                GameActionManager.instance.QueueAction(newHour);
            }

            if (dayRefresh)
            {
                GameActionManager.instance.QueueAction(newDay);
            }

            int nowYearHour = ((int)season * 30 - 30 + day - 1) * 24 + hour;
            seasonValue = nowYearHour / totalYearHour;
            Shader.SetGlobalFloat("_SeasonValue", SeasonValue);
        }
        const float totalYearHour = (4 * 30) * 24;

        float fixedSeason = -1;
        public float SeasonValue => fixedSeason < 0 ? seasonValue : fixedSeason;
        public void SetFixedSeason(SetFixedSeason SetFixedSeason)
        {
            fixedSeason = SetFixedSeason.season;
            int nowYearHour = ((int)season * 30 - 30 + day - 1) * 24 + hour;
            seasonValue = nowYearHour / totalYearHour;
            Shader.SetGlobalFloat("_SeasonValue", SeasonValue);
            EnvironmentManger.instance.ChangeWeatherDisplayType(SetFixedSeason.weatherDisplayType);
            WorldMapObjManager.instance.RefreshMapAudio();
        }
        private void UpDataGameTimeAction()
        {
            updateGame.year = year;
            updateGame.season = (int)season;
            updateGame.day = day;
            updateGame.hour = hour;
            updateGame.minute = minute;
            updateGame.totalMinute = minuteTime;
            GameActionManager.instance.QueueAction(updateGame);
        }

        private UpdateGameTime updateGame;

        public int minuteTime => (((year * 4 + (int)season) * 30 + day) * 24 + hour) * 60 + minute;
    }
    private GameTime nowGameTime;
    public int timeRunScale = 1;
#if UNITY_EDITOR
    public bool runTime = true;
#endif
    public bool night => nowGameTime.night;
    public float timeValue => nowGameTime.TimeValue;
    public float SeasonValue => nowGameTime.SeasonValue;
    public string NowGameTime => LanguageManage.instance.GameTimeToString(nowGameTime.year,nowGameTime.Season,nowGameTime.day);
    public int2 nowHourMinute=> new int2(nowGameTime.hour, nowGameTime.minute);
    public GameTimeKey nowGameTimeKey => new GameTimeKey(nowGameTime.hour, nowGameTime.minute, nowGameTime.hour, nowGameTime.minute);
    public int Year => nowGameTime.year;
    public Season Season => nowGameTime.Season;
    public Week Week => nowGameTime.week;
    public int Day => nowGameTime.day;

    public int GameDay
    {
        get
        {
            int d_year = Year-1;
            int d_season = (int)Season - (int)Season.夏;
            int d_day = Day - 1;
            return (d_year*4+d_season)*30+d_day;
        }
    }
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

    public int totalMinute => nowGameTime.minuteTime;

    public void SetTime(int hour = -1, int minute = -1)
    {
        if (nowGameTime != null)
        {
            nowGameTime.SetTime(hour, minute);
        }
    }
    public void SetDate(int day)
    {
        if (nowGameTime != null)
        {
            nowGameTime.SetDate(day);
        }
    }
    private Dictionary<int, GameDate> gameDates = new Dictionary<int, GameDate>();

    public List<GameDate> GetGameDataForSeason(Season season)
    {
        List<GameDate> results = new List<GameDate>();
        for (int i = 1; i <= 30; i++)
        {
            int key = (int)season * 100 + i;
            if (gameDates.TryGetValue(key, out var gameDate))
            {
                results.Add(gameDate);
            }
        }
        return results;
    }

    protected override void Clear()
    {
        base.Clear();
        gameDates.Clear();
    }

    public override void Init()
    {
        base.Init();
        gameDates.Clear();
        GameActionManager.instance.AddListener<LerpGameTime>(LerpGameTime);
        GameActionManager.instance.AddListener<SetMapOverrideEnvironment>(SetMapOverrideEnvironment);
        GameActionManager.instance.AddListener<ClearOverrideEnvironment>(ClearOverrideEnvironment);
        GameActionManager.instance.AddListener<PlayerSleep>(PlayerSleep);
        GameActionManager.instance.AddListener<CheckGameTimeDate>(CheckGameTimeDate);
        GameActionManager.instance.AddListener<SetFixedSeason>(SetFixedSeason);
        GameActionManager.instance.AddListener<TimeRun>(TimeRun);
        // CreatData();
    }

   
    void TimeRun(TimeRun TimeRun)
    {
        runTime = TimeRun.run;
    }
    void SetFixedSeason(SetFixedSeason SetFixedSeason)
    {
        nowGameTime.SetFixedSeason(SetFixedSeason);
    }
    private void CheckGameTimeDate(CheckGameTimeDate checkGameTimeDate)
    {
        bool result = false;
        if (checkGameTimeDate.year < Year)
        {
            result = true;
        }
        else if (checkGameTimeDate.year == Year)
        {
            if (checkGameTimeDate.momth < (int)Season)
            {
                result = true;
            }
            else if (checkGameTimeDate.momth == (int)Season)
            {
                if (checkGameTimeDate.day < Day)
                {
                    result = true;
                }
            }
        }
        if (checkGameTimeDate.setResult != null)
        {
            checkGameTimeDate.setResult(result);
        }
    }

    private void PlayerSleep(PlayerSleep playerSleep)
    {
        int sleepHour = playerSleep.targetHour - nowGameTime.hour;
        if (sleepHour < 0)
        {
            sleepHour += 24;
        }
        int characterId = playerSleep.characterId;
        bool isController = CharacterManager.instance.controllerCharacter.instanceId == characterId;

        void WakeUp()
        {
           // Debug.Log($"characterId:{characterId}");
            Character character = CharacterManager.instance.GetCharacter(characterId);
            PlayerWakeUp playerWakeUp = new PlayerWakeUp
            {
                characterId = playerSleep.characterId
            };
            GameActionManager.instance.QueueAction(playerWakeUp, true);
            SetCharacterAnimator setCharacterAnimator = new SetCharacterAnimator
            {
                characterId = playerSleep.characterId,
                parameter = "State",
                parameterType = ParameterType.INT,
                intValue = 0
            };
            GameActionManager.instance.QueueAction(setCharacterAnimator, true);

            SetCharacterAnimator setCharacterAnimatorDir_X = new SetCharacterAnimator
            {
                characterId = playerSleep.characterId,
                parameter = "Dir_X",
                parameterType = ParameterType.FLOAT,
                floatValue = 0
            };
            GameActionManager.instance.QueueAction(setCharacterAnimatorDir_X, true);
            SetCharacterAnimator setCharacterAnimatorDir_Y = new SetCharacterAnimator
            {
                characterId = playerSleep.characterId,
                parameter = "Dir_Y",
                parameterType = ParameterType.FLOAT,
                floatValue = -1
            };
            GameActionManager.instance.QueueAction(setCharacterAnimatorDir_Y, true);

            ChangeCharacterProperty changeCharacterProperty = new ChangeCharacterProperty
            {
                characterId = playerSleep.characterId,
                propertyType = CharacterPropertyType.体力,
                changeValue = (int)(character.CharacterProperty.MaxPower * 0.1667f * sleepHour)//六小时睡满体力
            };
            GameActionManager.instance.QueueAction(changeCharacterProperty, true);

            GameTimerController.instance.DelayAction(1200,
                () =>
                {
                    SetCharacterRandomCoordinate setCharacterRandomCoordinate = new SetCharacterRandomCoordinate
                    {
                        characterId = playerSleep.characterId,
                        Coordinate = character.coordinate,
                        range = 6
                    };
                    GameActionManager.instance.QueueAction(setCharacterRandomCoordinate);

                    if (isController)
                    {
                        OpenOrCloseInputMap openOrCloseInputMap = new OpenOrCloseInputMap
                        {
                            open = true,
                        };
                        GameActionManager.instance.QueueAction(openOrCloseInputMap);
                    }
                });
        }
        if (isController)
        {
            UIManager.instance.CloseGamePanel<OperateButtonPanel>();
            LerpGameTime(playerSleep.targetHour, playerSleep.targetMinute, GameCommon.sleepCostTime, true, WakeUp);
        }
        else
        {
            int year = nowGameTime.year;
            int season = (int)nowGameTime.Season;
            int day = nowGameTime.day;
            int hour = playerSleep.targetHour;
            if (hour < nowGameTime.hour)
            {
                day++;
                if (day > 30)
                {
                    day = 1;
                    season++;
                    if (season > 4)
                    {
                        season = 1;
                        year++;
                    }
                }
            }
            int minute = playerSleep.targetMinute;
            GameActionManager.instance.AddListener<UpdateGameTime>(UpdateGameTime);

            void UpdateGameTime(UpdateGameTime updateGameTime)
            {
                if (GameCommon.CompareGameTime(updateGameTime.year, updateGameTime.season, updateGameTime.day,
                    updateGameTime.hour, updateGameTime.minute, year, season, day, hour, minute))
                {
                    WakeUp();
                    GameActionManager.instance.RemoveListener<UpdateGameTime>(UpdateGameTime);
                }
            }
        }
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
                Season = Season.夏, 
            };
            nowGameTime.SetTime(6, 0);
            // StartTimeRun();
        }
    }

    public string GameTimeToString()
    {
        string timeStr = nowGameTime.year.ToString();
        timeStr += "," + nowGameTime.Season;
        timeStr += "," + nowGameTime.day;
        return timeStr;
    }

    public void StringToGameTime(string timeStr)
    {
        var x = timeStr.Split(',');
        nowGameTime.year = int.Parse(x[0]);
        nowGameTime.Season = (Season)Enum.Parse(typeof(Season), x[1]);
        nowGameTime.day = int.Parse(x[2]);
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
            gameDates.Add(gameDate.Key, gameDate);
        }
        //timeDisplayAction.UpdataTime();
    }

    private IEnumerator TimeRunIEnumerator;

    public void StartTimeRun()
    {
        StopTimeRun();
        TimeRunIEnumerator = TimeRun();
        GameObjectCurveController.instance.StartIEnumerator(TimeRunIEnumerator);
    }

    public void StopTimeRun()
    {
        if (TimeRunIEnumerator != null)
        {
            GameObjectCurveController.instance.StopIEnumerator(TimeRunIEnumerator);
        }
    }

    public void InitSaveDate(GameDateSaveData dateData)
    {
        nowGameTime.year = dateData.year;
        nowGameTime.day = dateData.day;

        nowGameTime.Season = dateData.season;
        nowGameTime.week = dateData.week;
        nowGameTime.SetTime(dateData.hour, dateData.minute);

        if (nowGameTime.day <= 15)
        {
            float moonOffSet = nowGameTime.day / 15.0f;
            Shader.SetGlobalFloat("_moonOffSet", moonOffSet);
        }
        else
        {
            float moonOffSet = (30 - nowGameTime.day) / 15.0f;
            Shader.SetGlobalFloat("_moonOffSet", moonOffSet);
        }
    }

    private void LerpGameTime(int targetHour, int targetMinute, float costTime, bool endRun = false, Action endAction = null)
    {
        StopTimeRun();
        var lerpTimeIEnumerator = LerpTime(targetHour, targetMinute, costTime, endRun, endAction);
        GameObjectCurveController.instance.StartIEnumerator(lerpTimeIEnumerator);
    }

    private void LerpGameTime(LerpGameTime lerpGameTime)
    {
        StopTimeRun();
        var lerpTimeIEnumerator = LerpTime(lerpGameTime.targetHour, lerpGameTime.targetMinute, lerpGameTime.totalTime);
        GameObjectCurveController.instance.StartIEnumerator(lerpTimeIEnumerator);
    }

    private IEnumerator LerpTime(int targetHour, int targetMinue, float totalTime, bool endRun = false, Action endAction = null)
    {
        int startValue = (Hour * 60 + Minute) * 20;
        if (targetHour < Hour)
        {
            targetHour += 24;
        }
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
        if (endRun)
        {
            StartTimeRun();
        }
        if (endAction != null)
        {
            endAction.Invoke();
        }
    }

    private IEnumerator TimeRun()
    {
        var  waitFixedUpdate=new WaitForFixedUpdate(); 
        bool timeRun = false;
        while (true)
        {
            for(int i = 0; i < timeRunScale; i++)
            {
                if (timeRun)
                {
                    nowGameTime.TimeRun();
                }
                timeRun = !timeRun;
            } 
            //timeDisplayAction.UpdataTime();
            yield return waitFixedUpdate;
        }
    }
}