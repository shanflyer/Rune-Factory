using System;
using Unity.Mathematics;
using UnityEngine;
[Serializable]
public struct WindData
{
    public WindParticleData left, right;
    public AudioSource audioSource;
    float volume;
    public void InitParticle()
    {
        left.InitParticle();
        right.InitParticle();
        volume = audioSource.volume;
    }
    public void SetValue(float value, bool immediatelyStop = false)
    {
        audioSource.volume = volume * value;
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
    public ParticleSystem wind, spring,summer,autumn,winter, smoke; 
    ParticleSystem.EmissionModule windEmission;
    ParticleSystem.EmissionModule springEmission,summerEmission,autumnEmission,winterEmission;
    ParticleSystem.EmissionModule smokeEmission;

    float windValue, springValue,summerValue,autumnValue,winterValue, smokeValue; 
    public void InitParticle()
    {
        windEmission = wind.emission;
        springEmission = spring.emission;
        summerEmission = summer.emission;
        autumnEmission = autumn.emission;
        winterEmission = winter.emission;
        smokeEmission = smoke.emission;

        windValue = windEmission.rateOverTime.constant;
        springValue = springEmission.rateOverTime.constant;
        summerValue = summerEmission.rateOverTime.constant;
        autumnValue = autumnEmission.rateOverTime.constant;
        winterValue = winterEmission.rateOverTime.constant;

        smokeValue = smokeEmission.rateOverTime.constant;

        windEmission.enabled = false;
        springEmission.enabled= false; 
        summerEmission.enabled= false; 
        autumnEmission.enabled= false;
        winterEmission.enabled= false; 
        smokeEmission.enabled = false;

    }
    public void SetValue(float value, bool immediatelyStop = false)
    {
        springEmission.enabled = summerEmission.enabled = autumnEmission.enabled =
            winterEmission.enabled = smokeEmission.enabled = false;
        float seasonValue = GameTimeManager.instance.SeasonValue;
        if (seasonValue < 0.1f)
        {
            winterEmission.enabled = value > 0;
            winterEmission.rateOverTime = math.lerp(0, winterValue, value);
        }
        else
        if (seasonValue>0.1f&&seasonValue<1)
        {
            springEmission.enabled = value > 0;
            springEmission.rateOverTime = math.lerp(0, springValue, value);
        }else if (seasonValue < 2.25f)
        {
            summerEmission.enabled = value > 0;
            summerEmission.rateOverTime = math.lerp(0, summerValue, value);
        }else if(seasonValue<3.3)
        {
            autumnEmission.enabled = value > 0;
            autumnEmission.rateOverTime = math.lerp(0, autumnValue, value);
        }
        else
        {
            winterEmission.enabled = value > 0;
            winterEmission.rateOverTime=math.lerp(0,winterValue, value);
        }

        windEmission.enabled =  value > 0;
        windEmission.rateOverTime = math.lerp(0, windValue, value);
        smokeEmission.enabled = value >= 0.25f;
        float _value = (value - 0.25f) * 1.334f; 
        
        smokeEmission.rateOverTime=math.lerp(0,smokeValue, _value);
    }
}
[Serializable]
public struct RainParticleData
{
    public ParticleSystem rain,drop,clouds;
    public AudioSource audioSource;
    ParticleSystem.EmissionModule rainEmission;
    ParticleSystem.EmissionModule dropEmission;
    ParticleSystem.EmissionModule cloudsEmission;
    ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime;
    float rainValue, dropValue, cloudsValue;
    float volume;
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
        volume = audioSource.volume;
    }
    public void SetValue(float value, bool immediatelyStop = false)
    {
        rainEmission.enabled=dropEmission.enabled=cloudsEmission.enabled = value > 0;
        rainEmission.rateOverTime = math.lerp(0, rainValue, value);
        dropEmission.rateOverTime=math.lerp(0, dropValue, value);
        cloudsEmission.rateOverTime=math.lerp(0,cloudsValue, value);
        audioSource.volume = volume * value;
    }
    public void SetWindValue(float value, bool immediatelyStop = false)
    {
        velocityOverLifetime.x =- 3 * value;
    }
}
[Serializable]
public struct FogParticleData
{ 
    public ParticleSystem fog;

    ParticleSystem.EmissionModule fogParticleEmission; 
    ParticleSystem.MainModule fogParticleMain;
    float mainColor_a;

    public void SetValue(float value, bool immediatelyStop = false)
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
    public void SetValue(float value, bool immediatelyStop = false)
    {
        emission.enabled = value > 0;
        emission.rateOverTime=math.lerp(0,snowEmission,value);
    }
    public void SetWindValue(float value, bool immediatelyStop = false)
    {
        var _lifeTime = lifeTime * math.lerp(1.0f, 0.4f, math.abs(value));
        mainModule.startLifetime=new ParticleSystem.MinMaxCurve(_lifeTime.x,_lifeTime.y);
        velocityOverLifetime.orbitalY = 0.03f * value;
        velocityOverLifetime.orbitalX = -0.01f * math.abs(value);
    }
}
public class WeatherMono : MonoBehaviour,IGameData
{
    [SerializeField]
    FogParticleData fogParticle; 
    [SerializeField]
    SnowParticleData snowData;
    [SerializeField]
    WindData windData;
    [SerializeField]
    RainParticleData rainParticle;

    public void SetFog(float fogValue, bool immediatelyStop = false)
    {
        fogParticle.SetValue(fogValue);
        if(immediatelyStop&&fogValue == 0)
        {
            fogParticle.fog.Clear();
        }
        else if (fogParticle.fog.isStopped)
        {
            fogParticle.fog.Play();
        }
    }
    public void SetSnow(float snowValue, bool immediatelyStop = false)
    {
        snowData.SetValue(snowValue);
        if (immediatelyStop && snowValue == 0)
        {
            snowData.snow.Clear(true); 
        }
        else if (snowData.snow.isStopped)
        {
            snowData.snow.Play();
        }
    }
    public void SetRain(float rainValue, bool immediatelyStop = false)
    {
        rainParticle.SetValue(rainValue);
        if (immediatelyStop && rainValue == 0)
        {
            rainParticle.rain.Clear();
            rainParticle.drop.Clear();
            rainParticle.clouds.Clear();
        }else if (rainParticle.rain.isStopped)
        {
            rainParticle.rain.Play();
            rainParticle.drop.Play();
            rainParticle.clouds.Play();
        }
    }
    public void SetWind(float windValue, bool immediatelyStop = false)
    {
       // float value = math.abs(windValue);
       // value = math.lerp(0.5f, 3, value);
       // value = windValue < 0 ? value : -value;
        Shader.SetGlobalFloat("_WindValue", windValue);

        windData.SetValue(windValue);
        rainParticle.SetWindValue(windValue);
        snowData.SetWindValue(windValue);
        if (immediatelyStop && windValue == 0)
        {
            windData.right.wind.Clear();
            windData.right.smoke.Clear();
            windData.left.wind.Clear();
            windData.left.smoke.Clear();

            windData.right.winter.Clear();
            windData.left.winter.Clear();
            windData.right.summer.Clear();
            windData.left.summer.Clear();
            windData.left.spring.Clear();
            windData.right.spring.Clear();
            windData.left.autumn.Clear();
            windData.right.autumn.Clear();
        }
        else if (windData.right.wind.isStopped)
        {
            windData.right.wind.Play();
            windData.right.smoke.Play();
            windData.left.wind.Play();
            windData.left.smoke.Play();

            windData.right.winter.Play();
            windData.left.winter.Play();
            windData.right.summer.Play();
            windData.left.summer.Play();
            windData.left.spring.Play();
            windData.right.spring.Play();
            windData.left.autumn.Play();
            windData.right.autumn.Play();
        }
        
    }

    public void Play()
    {
        ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
       

        fogParticle.fog.Play();
        snowData.snow.Play();
        rainParticle.rain.Play();
        rainParticle.drop.Play();
        rainParticle.clouds.Play();
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

        windData.audioSource = transform.Find("Wind").GetComponent<AudioSource>();
        ParticleSystem left = transform.Find("Wind/Left").GetComponent<ParticleSystem>();
        windData.left.wind = left;
        ParticleSystem leftSpring = transform.Find("Wind/Left/Spring").GetComponent<ParticleSystem>();
        windData.left.spring = leftSpring;
        ParticleSystem leftSummer = transform.Find("Wind/Left/Summer").GetComponent<ParticleSystem>();
        windData.left.summer = leftSummer;
        ParticleSystem leftAutumn = transform.Find("Wind/Left/Autumn").GetComponent<ParticleSystem>();
        windData.left.autumn = leftAutumn;
        ParticleSystem leftWinter = transform.Find("Wind/Left/Winter").GetComponent<ParticleSystem>();
        windData.left.winter = leftWinter;
        ParticleSystem smoke = transform.Find("Wind/Left/SmokeWhiteSoft").GetComponent<ParticleSystem>();
        windData.left.smoke = smoke;

        ParticleSystem right = transform.Find("Wind/Right").GetComponent<ParticleSystem>();
        windData.right.wind = right;
        ParticleSystem rightSpring = transform.Find("Wind/Right/Spring").GetComponent<ParticleSystem>();
        windData.right.spring= rightSpring;
        ParticleSystem rightSummer = transform.Find("Wind/Right/Summer").GetComponent<ParticleSystem>();
        windData.right.summer = rightSummer;
        ParticleSystem rightAutumn = transform.Find("Wind/Right/Autumn").GetComponent<ParticleSystem>();
        windData.right.autumn = rightAutumn;
        ParticleSystem rightWinter = transform.Find("Wind/Right/Winter").GetComponent<ParticleSystem>();
        windData.right.winter= rightWinter;

        ParticleSystem rightSmoke = transform.Find("Wind/Right/SmokeWhiteSoft").GetComponent<ParticleSystem>();
        windData.right.smoke = rightSmoke;

        ParticleSystem rain = transform.Find("Rain").GetComponent<ParticleSystem>();
        rainParticle.rain = rain;
        rainParticle.audioSource = rain.GetComponent<AudioSource>();
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
