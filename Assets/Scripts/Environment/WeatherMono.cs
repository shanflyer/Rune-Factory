using System;
using Unity.Mathematics;
using UnityEngine;
[Serializable]
public struct WindData
{
    public WindParticleData left, right;
    public void InitParticle()
    {
        left.InitParticle();
        right.InitParticle();
    }
    public void SetValue(float value)
    {
        if (value == 0)
        {
            left.SetValue(0);
            right.SetValue(0);
            return;
        }
        if (value < 0)
        {
            left.SetValue(-value);
            right.SetValue(0);
        }
        else
        {
            left.SetValue(0);
            right.SetValue(value);
        }
    }
}
[Serializable]
public struct WindParticleData
{
    public ParticleSystem wind, leave, smoke;

    ParticleSystem.EmissionModule windEmission;
    ParticleSystem.EmissionModule leaveEmission;
    ParticleSystem.EmissionModule smokeEmission;

    float windValue, leaveValue, smokeValue;
    public void InitParticle()
    {
        windEmission = wind.emission;
        leaveEmission = leave.emission;
        smokeEmission = smoke.emission;

        windValue = windEmission.rateOverTime.constant;
        leaveValue = leaveEmission.rateOverTime.constant;
        smokeValue = smokeEmission.rateOverTime.constant;

        windEmission.enabled = leaveEmission.enabled = smokeEmission.enabled = false;
    }
    public void SetValue(float value)
    {
        windEmission.enabled = leaveEmission.enabled = smokeEmission.enabled = value > 0;
        windEmission.rateOverTime = math.lerp(0, windValue, value);
        leaveEmission.rateOverTime = math.lerp(0, leaveValue, value);
        smokeEmission.rateOverTime=math.lerp(0,smokeValue, value);
    }
}
[Serializable]
public struct RainParticleData
{
    public ParticleSystem rain,drop,clouds;

    ParticleSystem.EmissionModule rainEmission;
    ParticleSystem.EmissionModule dropEmission;
    ParticleSystem.EmissionModule cloudsEmission;
    ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime;
    float rainValue, dropValue, cloudsValue;
    public void InitParticle()
    {
        rainEmission = rain.emission;
        dropEmission = drop.emission;
        cloudsEmission = clouds.emission;

        rainValue = rainEmission.rateOverTime.constant;
        dropValue = dropEmission.rateOverTime.constant;
        cloudsValue = cloudsEmission.rateOverTime.constant;
        velocityOverLifetime = rain.velocityOverLifetime;
        velocityOverLifetime.x = 0;
        rainEmission.enabled = dropEmission.enabled = cloudsEmission.enabled = false;
    }
    public void SetValue(float value)
    {
        rainEmission.enabled=dropEmission.enabled=cloudsEmission.enabled = value > 0;
        rainEmission.rateOverTime = math.lerp(0, rainValue, value);
        dropEmission.rateOverTime=math.lerp(0, dropValue, value);
        cloudsEmission.rateOverTime=math.lerp(0,cloudsValue, value);
    }
    public void SetWindValue(float value)
    {
        velocityOverLifetime.x = 3 * value;
    }
}
[Serializable]
public struct FogParticleData
{ 
    public ParticleSystem fog;

    ParticleSystem.EmissionModule fogParticleEmission; 
    ParticleSystem.MainModule fogParticleMain;
    float mainColor_a;

    public void SetValue(float value)
    {
        fogParticleEmission.enabled = value > 0;
        float now_a=math.lerp(0,mainColor_a,value);
        fogParticleMain.startColor = new Color(1, 1, 1, now_a); 
    }
    public void InitParticle()
    {
        fogParticleEmission = fog.emission;
        fogParticleMain = fog.main;
        mainColor_a = fogParticleMain.startColor.color.a;

        fogParticleEmission.enabled = false;
    }
}
[Serializable]
public struct SnowParticleData
{
    public ParticleSystem snow;
    ParticleSystem.EmissionModule emission;
    ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime;
    ParticleSystem.MainModule mainModule;
    float snowEmission;
    float2 lifeTime;
   

    public void InitParticle()
    {
        mainModule = snow.main;
        emission = snow.emission;
        snowEmission = emission.rateOverTime.constant;

        lifeTime = new float2(mainModule.startLifetime.constantMin, mainModule.startLifetime.constantMax);
        velocityOverLifetime = snow.velocityOverLifetime;
        emission.enabled = false;
    }
    public void SetValue(float value)
    {
        emission.enabled = value > 0;
        emission.rateOverTime=math.lerp(0,snowEmission,value);
    }
    public void SetWindValue(float value)
    {
        var _lifeTime = lifeTime * math.lerp(1.0f, 0.4f, math.abs(value));
        mainModule.startLifetime=new ParticleSystem.MinMaxCurve(_lifeTime.x,_lifeTime.y);
        velocityOverLifetime.y = 0.03f * value;
        velocityOverLifetime.x = -0.01f * value;
    }
}
public class WeatherMono : MonoBehaviour
{
    [SerializeField]
    FogParticleData fogParticle; 
    [SerializeField]
    SnowParticleData snowData;
    [SerializeField]
    WindData windData;
    [SerializeField]
    RainParticleData rainParticle;

    public void SetFog(float fogValue)
    {
        fogParticle.SetValue(fogValue);
    }
    public void SetSnow(float snowValue)
    {
        snowData.SetValue(snowValue);
    }
    public void SetRain(float rainValue)
    {
        rainParticle.SetValue(rainValue);
    }
    public void SetWind(float windValue)
    {
        windData.SetValue(windValue);
        rainParticle.SetWindValue(windValue);
        snowData.SetWindValue(windValue);
    }

    public string GetKey()
    {
        return "WeatherMono";
    }

    public void SetReferenceData()
    {
        ParticleSystem fog = transform.Find("Fog/Fog").GetComponent<ParticleSystem>();
        fogParticle.fog = fog;
        var snow = transform.Find("Snow").GetComponent<ParticleSystem>();
        snowData.snow = snow;

        ParticleSystem left = transform.Find("Wind/Left").GetComponent<ParticleSystem>();
        windData.left.wind = left;
        ParticleSystem leave = transform.Find("Wind/Left/BlowingLeaves").GetComponent<ParticleSystem>();
        windData.left.leave = leave;
        ParticleSystem smoke = transform.Find("Wind/Left/SmokeWhiteSoft").GetComponent<ParticleSystem>();
        windData.left.smoke = smoke;

        ParticleSystem right = transform.Find("Wind/Right").GetComponent<ParticleSystem>();
        windData.right.wind = right;
        ParticleSystem rightLeave = transform.Find("Wind/Right/BlowingLeaves").GetComponent<ParticleSystem>();
        windData.right.leave = rightLeave;
        ParticleSystem rightSmoke = transform.Find("Wind/Right/SmokeWhiteSoft").GetComponent<ParticleSystem>();
        windData.right.smoke = rightSmoke;

        ParticleSystem rain = transform.Find("Rain").GetComponent<ParticleSystem>();
        rainParticle.rain = rain;
        ParticleSystem drop = transform.Find("Rain/Drop").GetComponent<ParticleSystem>();
        rainParticle.drop = drop;
        ParticleSystem clouds = transform.Find("Rain/Clouds").GetComponent<ParticleSystem>();
        rainParticle.clouds=clouds;

    }
    private void Awake()
    {
        windData.InitParticle();
        rainParticle.InitParticle();
        fogParticle.InitParticle();
        snowData.InitParticle();
    }

 
}
