using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct Weather
{
    public float cloud;
    public float temperature;
    public float fog;
    public float wind;
    public float waterFall;
    public float lightning;

    public static Weather Lerp(Weather weather0, Weather weather1, float value)
    {
        Weather weather = new Weather
        {
            cloud = math.lerp(weather0.cloud, weather1.cloud, value),
            temperature = math.lerp(weather0.temperature, weather1.temperature, value),
            fog = math.lerp(weather0.fog, weather1.fog, value),
            wind = math.lerp(weather0.wind, weather1.wind, value),
            waterFall = math.lerp(weather0.waterFall, weather1.waterFall, value),
            lightning = math.lerp(weather0.lightning, weather1.lightning, value),
        };
        return weather;
    }

    public bool IsSnow()
    {
        float seasonValue = GameTimeManager.instance.SeasonValue;
        bool snow = seasonValue >= 3 || seasonValue < 0.05f;
        return snow;
    }

    public float GetWeatherLight()
    {
        float fogValue = (1 - fog) * 0.25f + 0.75f;
        float cloudValue = (1 - cloud) * 0.25f + 0.75f;
        float waterFallValue = (1 - waterFall) * 0.5f + 0.5f;
        if (waterFall > 0.5f)
        {
            return waterFallValue;
        }
        else
        {
            return fogValue * cloudValue;
        }
    }

    public float GetFlareLight()
    {
        float fogValue = 1.0f - fog * 2;
        fogValue = fogValue < 0 ? 0 : fogValue;

        float cloudValue = 1.0f - cloud * 2;
        cloudValue = cloudValue < 0 ? 0 : cloudValue;

        float waterFallValue = 1.0f - waterFall * 4;
        waterFallValue = waterFallValue < 0 ? 0 : waterFallValue;

        float timelightValue = GameTimeManager.instance.timeLightValue;

        if (fogValue > cloudValue || fogValue > waterFallValue)
        {
            if (cloudValue > waterFallValue)
            {
                return waterFallValue* timelightValue;
            }
            else
            {
                return cloudValue * timelightValue; ;
            }
        }
        return fogValue * timelightValue;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(cloud, temperature, fog, wind, waterFall, lightning);
    }

    public override bool Equals(object obj)
    {
        if (obj is Weather weather)
        {
            return weather.cloud == cloud && weather.temperature == temperature && weather.fog == fog && weather.wind == wind && weather.waterFall == waterFall &&
           weather.lightning == lightning;
        }
        return false;
    }
    public static bool operator ==(Weather weather0, Weather weather1)
    {
        return weather0.cloud == weather1.cloud && weather0.temperature == weather1.temperature && weather0.fog == weather1.fog && weather0.wind == weather1.wind && weather0.waterFall == weather1.waterFall &&
            weather0.lightning == weather1.lightning;
    }
    public static bool operator !=(Weather weather0, Weather weather1)
    {
        return weather0.cloud != weather1.cloud || weather0.temperature != weather1.temperature || weather0.fog != weather1.fog || weather0.wind != weather1.wind || weather0.waterFall != weather1.waterFall ||
            weather0.lightning != weather1.lightning;
    }
}

public class WeatherManager : Singleton<WeatherManager>
{
    private List<Weather> nowDayWeathers = new List<Weather>();
    private List<Weather> nextDayWeathers = new List<Weather>();
    WeatherIconData weatherIconData;
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;
    public override IReadOnlyList<Type> InitializationDependencies => new[] { typeof(GameSourceManager), typeof(GameDataManager), typeof(GameRandom) };

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        weatherIconData = await GameSourceManager.instance.GetSingleScriptableObject<WeatherIconData>("Data/WeatherIconData");
        if (weatherIconData == null)
        {
            Debug.LogError("WeatherManager init failed: missing Data/WeatherIconData.");
            return;
        }

        weatherIconData.InitData();
        GameActionManager.instance.AddAsyncListener<CreatWeather>(CreatWeatherAsync, nameof(CreatWeather));
    }

    protected override void Clear()
    {
        initializationTask = Task.CompletedTask;
        base.Clear();
    }
    public List<WeatherReferenceData> GetNowWeatherReferenceDatas()
    {
        List<WeatherReferenceData> weatherReferenceDatas = new List<WeatherReferenceData>();
        for(int i = 0; i < nowDayWeathers.Count; i++)
        {
            WeatherReferenceData weatherReferenceData = new WeatherReferenceData
            {
                time = $"{(3 * i+3).ToString("00")}:00",
                night=i==0||i>4,
                weather = nowDayWeathers[i]
            };
            weatherReferenceDatas.Add(weatherReferenceData);
        }
        return weatherReferenceDatas;
    }
    public List<WeatherReferenceData> GetNextWeatherReferenceDatas()
    {
        List<WeatherReferenceData> weatherReferenceDatas = new List<WeatherReferenceData>();
        for (int i = 0; i < nextDayWeathers.Count; i++)
        {
            WeatherReferenceData weatherReferenceData = new WeatherReferenceData
            {
                time = $"{(3 * i + 3).ToString("00")}:00",
                night = i == 0 || i > 4,
                weather = nextDayWeathers[i]
            };
            weatherReferenceDatas.Add(weatherReferenceData);
        }
        return weatherReferenceDatas;
    }
    public Sprite GetWeatherIcon(Weather weather, bool overrideNight = false, bool night = false)
    {
       return  weatherIconData.GetWeatherIcon(weather,overrideNight,night);
    }

    public Sprite GetWeatherIcon(bool overrideNight = false, bool night = false)
    {
        return weatherIconData.GetWeatherIcon(nowWeather, overrideNight, night);
    }
    public void InitSaveWeather(List<Weather> nowDayWeathers, List<Weather> nextDayWeathers)
    {
        this.nowDayWeathers = nowDayWeathers;
        this.nextDayWeathers = nextDayWeathers;
    }

    private async System.Threading.Tasks.Task CreatWeatherAsync(CreatWeather creatWeather)
    {
        if (creatWeather.nowWeathers != null && creatWeather.nowWeathers.Count > 0)
        {
            nowDayWeathers = await CreatWeather(creatWeather.nowWeathers);
            GameDataSaveManager.instance.UserGameSaveData.nowWeathers = nowDayWeathers;
        }
        else
        {
            nowDayWeathers.Clear();
            nowDayWeathers.AddRange(nextDayWeathers);
            GameDataSaveManager.instance.UserGameSaveData.nowWeathers = nowDayWeathers;
            RefreshWeather(GameTimeManager.instance.Hour);
        }
        if (creatWeather.nextWeather != null && creatWeather.nextWeather.Count > 0)
        {
            nextDayWeathers = await CreatWeather(creatWeather.nextWeather);
            GameDataSaveManager.instance.UserGameSaveData.nextWeathers = nextDayWeathers;

        }
    }

    private async Task<List<Weather>> CreatWeather(List<int> weatherDatas)
    {
        List<Weather> weathers = new List<Weather>();
        for (int i = 0; i < weatherDatas.Count; i++)
        {
            WeatherData weatherData = await GameDataManager.instance.GetAsyncData<WeatherData>(weatherDatas[i]);
            Weather weather = new Weather
            {
                cloud = GameRandom.RandomFloat(weatherData.cloud),
                waterFall = GameRandom.RandomFloat(weatherData.waterFall),
                wind = GameRandom.RandomFloat(weatherData.windStrength),
                fog = GameRandom.RandomFloat(weatherData.fog),
                lightning = GameRandom.RandomFloat(weatherData.lightning)
            };
            weathers.Add(weather);
        }
        return weathers;
    }

    private int nowIndex = 0;
    private Weather nowWeather;
    public void RefreshWeather(int hour)
    {
#if UNITY_EDITOR
        if (GameController.instance.autoWeather)
        {
            return;
        }
#endif
        int hourIndex = (int)math.floor(hour / 3.0f);
        if (hourIndex != nowIndex&& nowDayWeathers.Count>hourIndex)
        {
            nowIndex = hourIndex;
            nowWeather = nowDayWeathers[hourIndex];
            SetWeather setWeather = new SetWeather
            {
                weather = nowWeather
            };
            GameActionManager.instance.QueueAction(setWeather);
        }
    }
}
