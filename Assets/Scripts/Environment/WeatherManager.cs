 
using UnityEngine;
using Unity.Mathematics;
using System.Collections;

public struct Weather
{ 
    public float cloud;
    public float temperature;
    public float fog;
    public Vector3 wind;
    public float waterFall;
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
                wind = new float3(GameRandom.RandomFloat2(weatherData.windDir), GameRandom.RandomFloat(weatherData.windStrength)),
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
        Shader.SetGlobalFloat("_CloudValue", nowWeather.cloud);
        Shader.SetGlobalVector("_Wind", nowWeather.wind);
        Shader.SetGlobalFloat("_Fog",nowWeather.fog);
    }

    
}