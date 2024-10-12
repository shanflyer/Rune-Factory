 
using UnityEngine;
using Unity.Mathematics;
using System.Collections;
using System;

[Serializable]
public struct Weather
{ 
    public float cloud;
    public float temperature;
    public float fog;
    public float wind;
    public float waterFall;
    public float lightning;

    public static Weather Lerp(Weather weather0, Weather weather1,float value) 
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
        float fogValue =(1- fog) * 0.25f + 0.75f;
        float cloudValue = (1-cloud) * 0.25f + 0.75f;
        float waterFallValue=(1- waterFall) * 0.3f + 0.7f;
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
        float fogValue =1.0f- fog*2;
        fogValue = fogValue < 0 ? 0 : fogValue;
         
        float cloudValue = 1.0f - cloud * 2;
        cloudValue = cloudValue < 0 ? 0 : cloudValue;

        float waterFallValue = 1.0f - waterFall * 4;
        waterFallValue = waterFallValue < 0 ? 0 : waterFallValue;

        if (fogValue > cloudValue || fogValue > waterFallValue)
        {
            if(cloudValue> waterFallValue)
            {
                return waterFallValue;
            }
            else
            {
                return cloudValue;
            }
        }
        return fogValue;
    }
}
public class WeatherManager : Singleton<WeatherManager>
{
    Weather nowWeather;
    bool ZeroWeather;
    public override void Init()
    {
        base.Init();
        GameActionManager.instance.AddListener<WeatherAction>(WeatherAction);

    }
    async void WeatherAction(WeatherAction weatherAction)
    {
        WeatherData weatherData =await GameDataManager.instance.GetAsyncData<WeatherData>(weatherAction.weatherDataId);
        if (weatherData != null)
        {
            Weather nextWeather = new Weather
            {
                temperature = GameRandom.RandomFloat(weatherData.temperature),
                cloud = GameRandom.RandomFloat(weatherData.cloud),
                fog = GameRandom.RandomFloat(weatherData.fog),
                waterFall = GameRandom.RandomFloat(weatherData.rainfall),
                wind = GameRandom.RandomFloat(weatherData.windStrength),
            };
            if (!ZeroWeather)
            {
                ZeroWeather = true;
                nowWeather = nextWeather;
                ShowWeather();
            }
            else
            {
                var iEnumerator = LerpWeather(nextWeather);
                GameObjectCurveController.instance.StartIEnumerator(iEnumerator);
            }
            
        }
    }
    IEnumerator LerpWeather(Weather nextWeather)
    {
        float timeValue = 0;
        while(timeValue<GameCommon.weatherLerpTime)
        {
            timeValue += Time.deltaTime;
            float value = timeValue / GameCommon.weatherLerpTime;
            nowWeather.cloud = math.lerp(nowWeather.cloud, nextWeather.cloud,value);
            nowWeather.temperature=math.lerp(nowWeather.temperature,nextWeather.temperature,value);
            nowWeather.fog=math.lerp(nowWeather.fog,nextWeather.fog,value); 
            nowWeather.wind=math.lerp(nowWeather.wind,nextWeather.wind,value);
            nowWeather.waterFall=math.lerp(nowWeather.waterFall,nextWeather.waterFall,value);

            ShowWeather();
            yield return 0;
        }
    }
     
   
    void ShowWeather()
    {
        SetWeather setWeather = new SetWeather
        {
            weather = nowWeather
        };
        GameActionManager.instance.QueueAction(setWeather); 
    }

    
}