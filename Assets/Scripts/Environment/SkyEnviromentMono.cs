
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[Serializable]
public struct SkyCloudData
{
    public ParticleSystem particleSystem;
    public Color startColor, endColor; 

    ParticleSystem.MainModule mainModule;
    ParticleSystem.EmissionModule emissionModule;
    ParticleSystem.VelocityOverLifetimeModule VelocityOverLifetimeModule;
    ParticleSystemRenderer ParticleSystemRenderer;

    public void DisplayEnable(bool enable)
    {
        ParticleSystemRenderer.enabled = enable;
    }
    public void InitParticle()
    {
        mainModule = particleSystem.main;
        emissionModule = particleSystem.emission;
        VelocityOverLifetimeModule = particleSystem.velocityOverLifetime;
        ParticleSystemRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        SetCloudValue(0);
        SetWindValue(0);
    }

    public void SetCloudValue(float value)
    {
        Color color=Color.Lerp(startColor,endColor,value);
        mainModule.startColor = color; 
        emissionModule.rateOverTime=math.lerp(0.25f,30,value); 
    }
    public void SetWindValue(float windValue)
    {
        float speed = math.lerp(1, 3, math.abs(windValue));
        mainModule.simulationSpeed = speed;
        VelocityOverLifetimeModule.x =new ParticleSystem.MinMaxCurve( -windValue * 0.2f, -windValue * 0.2f);
    }
    
}

public class SkyEnviromentMono : MonoBehaviour, IGameData
{
    [SerializeField]
    ProFlare proFlare;
    [SerializeField]
    ProFlareBatch proFlareBatch;
    [SerializeField]
    SpriteRenderer sun;
    [SerializeField]
    Collider2D sunClollider;
    [SerializeField]
    SpriteRenderer sky;
    [SerializeField]
    SkyCloudData skyCloud;
    [SerializeField]
    private Light2D directionLight;
    [SerializeField]
    private Light2D globalLight;

    [SerializeField]
    SpriteRenderer bg,sea;
    [SerializeField]
    WeatherMono weatherMono;
    public ProFlare ProFlare
    {
        get
        {
            return proFlare;
        }
    }
    public ProFlareBatch ProFlareBatch
    {
        get
        {
            return proFlareBatch;
        }
    }
    public Transform Sun
    {
        get
        {
            if(sun == null)
            {
                return null;
            }
            return sun.transform;
        }
    }
    public Light2D GlobalLight=>globalLight;
    public Light2D DirectionLight=>directionLight;
    private void Awake()
    {
        skyCloud.InitParticle();
        GameActionManager.instance.AddListener<DisplaySky>(DisplaySky);
    }
    Vector2 skyBgStartPos, skyBgEndPos;
    Vector2 mapSize;
    float4 bgOffset;
    float2 cameraOffset;

    public SetFloatValue setWindValue;
    public void PlayWeather()
    {
       // weatherMono.Play();
    }
    async void DisplaySky(DisplaySky displaySky)
    {
        bgOffset =float4.zero;
        if (displaySky.display)
        {
            sky.enabled = true;
            sun.enabled = true;
            bg.enabled = true;
            sea.enabled = true;
            skyCloud.DisplayEnable(true); 
            sun.gameObject.SetActive(true);
            sunClollider.enabled = true;
            ProFlareBatch.gameObject.SetActive(true);
            ProFlareBatch.ForceRefresh();
        }
        else
        {
            sky.enabled = false;
            sun.enabled = false;
            bg.enabled = false;
            sea.enabled = false;
            skyCloud.DisplayEnable(false);
            sun.gameObject.SetActive(false);
            sunClollider.enabled = false;
            if (displaySky.displaySunlight)
            {
                ProFlareBatch.gameObject.SetActive(true);
                ProFlareBatch.SetTrigger2DGameObject(true);
                ProFlareBatch.ForceRefresh();
            }
            else
            {
                ProFlareBatch.gameObject.SetActive(false);
            }
        }

        var skyBackGroundData = await GameDataManager.instance.GetAsyncData<SkyBackGroundData>(displaySky.skyId);
        if (skyBackGroundData != null)
        {
            bg.sprite = skyBackGroundData.backGround;
            cameraOffset = skyBackGroundData.cameraOffset;
            bgOffset = skyBackGroundData.bgOffset;
        }
        else
        {
            bg.sprite = null;
            cameraOffset = 0;
            bgOffset = 0;
        }
        skyBgStartPos = displaySky.startPos;
        skyBgEndPos = displaySky.endPos;
        mapSize = skyBgEndPos - skyBgStartPos;
        SetBgPos(CameraManager.instance.mainCamera.transform.position);
    }

    public void SetBgPos(Vector2 cameraPos)
    {
        float2 offsetValue = (cameraPos - skyBgStartPos) / mapSize;
        offsetValue = math.clamp(offsetValue, 0, 1);

        Vector2 pos = new Vector2(math.lerp(bgOffset.x, bgOffset.z, offsetValue.x), math.lerp(bgOffset.y, bgOffset.w, offsetValue.y));
        bg.transform.localPosition = pos;

        float cameraOffsetY= math.lerp( cameraOffset.x, cameraOffset.y, offsetValue.y);
        CameraManager.instance.SetCameraOffset(new Vector2(0, cameraOffsetY));
    }

    public void SetWeather(Weather weather)
    { 
        if (weather.IsSnow())
        {
            weatherMono.SetRain(0);
            weatherMono.SetSnow(weather.waterFall);
        }
        else
        {
            weatherMono.SetSnow(0);
            weatherMono.SetRain(weather.waterFall);
        }
        weatherMono.SetFog(weather.fog);
        weatherMono.SetWind(weather.wind);
        float cloudValue = weather.waterFall * 3;
        float cloud = weather.cloud;
        if (cloudValue > weather.cloud)
        {
            cloud = cloudValue;
        }
        cloud = cloud > 1 ? 1 : cloud;
        skyCloud.SetCloudValue(cloud);
        skyCloud.SetWindValue(weather.wind);
        Shader.SetGlobalFloat("_CloudValue", cloud);

        if (setWindValue != null)
        {
            setWindValue(weather.wind);
        }
    }
    public void SetWeather(float waterFall,float fog,float wind,float cloud)
    {
        float seasonValue = GameTimeManager.instance.SeasonValue;
        bool snow = seasonValue >= 3 || seasonValue < 0.05f;
        if (snow)
        {
            weatherMono.SetRain(0);
            weatherMono.SetSnow(waterFall);
        }
        else
        {
            weatherMono.SetSnow(0);
            weatherMono.SetRain(waterFall);
        }
        weatherMono.SetFog(fog);
        weatherMono.SetWind(wind);
        float cloudValue = waterFall * 3;
        if (cloudValue > cloud)
        {
            cloud = cloudValue;
        }
        cloud = cloud > 1 ? 1 : cloud;
        skyCloud.SetCloudValue(cloud);
        skyCloud.SetWindValue(wind);
        Shader.SetGlobalFloat("_CloudValue", cloud);
        if (setWindValue != null)
        {
            setWindValue(wind);
        }
    }
    public string GetKey()
    {
        return ToString();
    }

    public void SetReferenceData()
    {
        weatherMono = transform.Find("Weather").GetComponent<WeatherMono>();
        weatherMono.SetReferenceData();
    }
}