
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
[CreateAssetMenu(menuName ="Data/天气Icon")]
public class WeatherIconData : ScriptableObject
{
    public int[] seasonDayWaterFall;
    public Sprite[] waterFallIcons;

    public int[] dayCloud;
    public Sprite[] weatherIcons;

    Dictionary<int, Sprite> waterFallIconDic;
    Dictionary<int, Sprite> cloudIconDic;
    public void InitData()
    {
        waterFallIconDic = new Dictionary<int, Sprite>();
        cloudIconDic = new Dictionary<int, Sprite>();

        for(int i = 0; i < seasonDayWaterFall.Length; i++)
        {
            waterFallIconDic[seasonDayWaterFall[i]] = waterFallIcons[i];
        }
        for(int i = 0; i < dayCloud.Length; i++)
        {
            cloudIconDic[dayCloud[i]] = weatherIcons[i];
        }
    }
    public Sprite GetWeatherIcon(Weather weather,bool overrideNight=false,bool night=false)
    {
        if (weather.waterFall > 0)
        {
            bool isSnow = weather.IsSnow();
            int snowIndex = isSnow ? 1 : 0;
            int waterFallIndex;
            if (weather.lightning > 0)
            {
                waterFallIndex = 3;
            }
            else
            {
                waterFallIndex = (int)(weather.waterFall * 3.0f);
                waterFallIndex = waterFallIndex > 2 ? 2 : waterFallIndex;
            }
            int dayIndex = GameTimeManager.instance.night ? 1 : 0;
            if (overrideNight)
            {
                dayIndex=night ? 1 : 0;
            }
            waterFallIconDic.TryGetValue(snowIndex*100+dayIndex*10+waterFallIndex, out var Sprite);
            return Sprite;
        }
        else
        {
            int cloudIndex = (int)(weather.cloud*6.0f);
            cloudIndex = cloudIndex > 2 ? 2:cloudIndex;
            int dayIndex = GameTimeManager.instance.night ? 1 : 0;
            if (overrideNight)
            {
                dayIndex = night ? 1 : 0;
            }
            cloudIconDic.TryGetValue(dayIndex * 10 + cloudIndex, out var Sprite);
            return Sprite;
        }       
    }
}

 