using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct Weather
{
    [NonSerialized]
    public float cloud;

    [NonSerialized]
    public float temperature;

    [NonSerialized]
    public float fog;

    [NonSerialized]
    public float wind;

    [NonSerialized]
    public float waterFall;

    [NonSerialized]
    public float lightning;

    // 压缩字段
    public ulong packed;

    private static int ToInt01(float value)
    {
        // 0~1 -> 0~1000
        var v = (int)Math.Round(value * 1000f);
        return Math.Clamp(v, 0, 1000);
    }

    private static float ToFloat01(int value)
    {
        return value / 1000f;
    }

    private static int ToIntSigned(float value)
    {
        // -1~1 -> 0~2000
        var v = (int)Math.Round((value + 1f) * 1000f);
        return Math.Clamp(v, 0, 2000);
    }

    private static float ToFloatSigned(int value)
    {
        // 0~2000 -> -1~1
        return value / 1000f - 1f;
    }

    // ========= 打包 =========
    public void Pack()
    {
        packed = 0;
        packed |= (ulong)ToInt01(cloud) << 0; // 10位
        packed |= (ulong)ToInt01(temperature) << 10; // 10位
        packed |= (ulong)ToInt01(fog) << 20; // 10位
        packed |= (ulong)ToIntSigned(wind) << 30; // 11位
        packed |= (ulong)ToInt01(waterFall) << 41; // 10位
        packed |= (ulong)ToInt01(lightning) << 51; // 10位
    }

    // ========= 解包 =========
    public void Unpack()
    {
        cloud = ToFloat01((int)((packed >> 0) & 0x3FFUL)); // 10位
        temperature = ToFloat01((int)((packed >> 10) & 0x3FFUL)); // 10位
        fog = ToFloat01((int)((packed >> 20) & 0x3FFUL)); // 10位
        wind = ToFloatSigned((int)((packed >> 30) & 0x7FFUL)); // 11位
        waterFall = ToFloat01((int)((packed >> 41) & 0x3FFUL)); // 10位
        lightning = ToFloat01((int)((packed >> 51) & 0x3FFUL)); // 10位
    }

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
    public override async void Init()
    {
        weatherIconData = await GameSourceManager.instance.GetSingleScriptableObject<WeatherIconData>("Data/WeatherIconData");
        weatherIconData.InitData();
        base.Init();
        GameActionManager.instance.AddListener<CreatWeather>(CreatWeather);
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

    private async void CreatWeather(CreatWeather creatWeather)
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