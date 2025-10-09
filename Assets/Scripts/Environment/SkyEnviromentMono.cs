using System;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[Serializable]
public struct SkyCloudData
{
    public ParticleSystem particleSystem;
    public Color startColor, endColor; 

    MainModule mainModule;
    EmissionModule emissionModule;
    VelocityOverLifetimeModule VelocityOverLifetimeModule;
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
        VelocityOverLifetimeModule.x =new MinMaxCurve( -windValue * 0.2f, -windValue * 0.2f);
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
    ParticleSystem farCloud;
    [SerializeField]
    ParticleSystem nearCloud;
    [SerializeField]
    ParticleSystem star;
    [SerializeField]
    AnimationCurve timeStarRange, dateStarRange;
    [SerializeField]
    Vector2 farCloudCount;
    [SerializeField]
    Vector2 nearCloudCount;
    [SerializeField]
    Vector4 farCloudSize;
    [SerializeField]
    Vector4 nearCloudSize;
    [SerializeField]
    Vector4 environmentLerpOffset;

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
    VelocityOverLifetimeModule farVelocity, nearVelocity;
    EmissionModule farEmission, nearEmission,starEmission;
    MainModule farMain, nearMain;
    private void Awake()
    { 
        GameActionManager.instance.AddListener<DisplaySky>(DisplaySky);
        GameActionManager.instance.AddListener<UpdateGameTime>(UpdateGameTime);
       
        farVelocity = farCloud.velocityOverLifetime;
        nearVelocity = nearCloud.velocityOverLifetime;
        farEmission = farCloud.emission;
        nearEmission = nearCloud.emission;
        farMain = farCloud.main;
        nearMain = nearCloud.main;
        starEmission = star.emission;
        starEmission.rateOverTime = 0;
        star.Clear();
        float screenHeight = Screen.height;
        float dt = screenHeight - environmentLerpOffset.x;
        dt *= environmentLerpOffset.y;
        dt = math.clamp(dt, environmentLerpOffset.z, environmentLerpOffset.w);
        transform.localPosition = new Vector3(0, dt, 0);
    }

    /*
    float cloudTime = 0;
    private void Update()
    {
        if (displayCloud)
        {
            cloudTime += Time.deltaTime;
            if (cloudTime > 0.5f)
            {
                cloudTime = 0;
                
               // farCloud.Emit((int)math.lerp(farCloudCount.x,farCloudCount.y,cloud));
               // nearCloud.Emit((int)math.lerp(nearCloudCount.x, nearCloudCount.y, cloud));
            }
        } 
    }*/

    Vector2 skyBgStartPos, skyBgEndPos;
    Vector2 mapSize;
    float4 bgOffset;
    float2 cameraOffset;

    public SetFloatValue setWindValue;
    bool displayCloud=false;
   
    void UpdateGameTime(UpdateGameTime updateGameTime)
    {
        float timeValue =timeStarRange.Evaluate(GameTimeManager.instance.timeValue);
        float dateValue = dateStarRange.Evaluate(updateGameTime.day/30.0f);
        starEmission.rateOverTime = timeValue * dateValue;
    }
    async void DisplaySky(DisplaySky displaySky)
    {
        displayCloud = !(!displaySky.display && !displaySky.displaySunlight);
        bgOffset =float4.zero;
        if (displaySky.display)
        {
            sky.enabled = true;
            sun.enabled = true;
            bg.enabled = true;
            sea.enabled = true;
            
            sun.gameObject.SetActive(true);
            sunClollider.enabled = true;
            ProFlareBatch.gameObject.SetActive(true);
            ProFlareBatch.ForceRefresh();

            farEmission.rateOverTime = (int)math.lerp(farCloudCount.x, farCloudCount.y, cloud);
            nearEmission.rateOverTime = (int)math.lerp(nearCloudCount.x, nearCloudCount.y, cloud);
            farCloud.Play();
            nearCloud.Play();
            star.Play();
        }
        else
        {
            sky.enabled = false;
            sun.enabled = false;
            bg.enabled = false;
            sea.enabled = false;
           
            sun.gameObject.SetActive(false);
            sunClollider.enabled = false;
            if (displaySky.displaySunlight)
            {
                farEmission.rateOverTime = (int)math.lerp(farCloudCount.x, farCloudCount.y, cloud);
                nearEmission.rateOverTime = (int)math.lerp(nearCloudCount.x, nearCloudCount.y, cloud);
                farCloud.Play();
                nearCloud.Play();
                star.Play();
                ProFlareBatch.gameObject.SetActive(true);
                ProFlareBatch.SetTrigger2DGameObject(true);
                ProFlareBatch.ForceRefresh();
            }
            else
            {
                farCloud.Stop();
                nearCloud.Stop();
                farCloud.Clear();
                nearCloud.Clear();
                
                star.Stop();
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

    public void ChangeWeatherDisplayType(WeatherDisplayType weatherDisplayType)
    {
        var inSide = weatherDisplayType == WeatherDisplayType.Inside;
        if (inSide)
        {
            farCloud.Stop();
            nearCloud.Stop();
        }

        weatherMono.HideWeather(inSide);
        weatherMono.ChangeWeatherAudio((int)weatherDisplayType);
    }

    float cloud
    {
        get
        {
            return _cloud;
        }
        set
        {
            _cloud = value;
            //Debug.Log($"Cloud:{value}");
            if (value > 0 && displayCloud)
            {
                if (farCloud.isStopped) farCloud.Play();

                if (nearCloud.isStopped) nearCloud.Play();
            }
            
            float farCount = math.lerp(farCloudCount.x, farCloudCount.y, cloud);
            if (farCount < farEmission.rateOverTime.constant)
            {
                Particle[] particles = new Particle[farCloud.particleCount];
                farCloud.GetParticles(particles);
                for (int i = 0; i < particles.Length; i++)
                {

                    if (particles[i].remainingLifetime > particles[i].startLifetime * 0.15f)
                    {
                        
                        particles[i].remainingLifetime = particles[i].startLifetime * 0.15f;
                    }

                }
                farCloud.SetParticles(particles);
            }

            farEmission.rateOverTime = farCount;


            float nearCount = math.lerp(nearCloudCount.x, nearCloudCount.y, cloud);
            if (nearCount < nearEmission.rateOverTime.constant)
            {
                Particle[] particles = new Particle[nearCloud.particleCount];
                nearCloud.GetParticles(particles);
                for (int i = 0; i < particles.Length; i++)
                {
                    if (particles[i].remainingLifetime > particles[i].startLifetime * 0.8f)
                    {
                        particles[i].remainingLifetime = 1- particles[i].remainingLifetime;
                    }else
                    if (particles[i].remainingLifetime > particles[i].startLifetime * 0.15f)
                    {
                        particles[i].remainingLifetime = particles[i].startLifetime * 0.15f;
                    }

                }
               nearCloud.SetParticles(particles);
            }
            nearEmission.rateOverTime = nearCount;


            var farStartSize = farMain.startSize;
            farStartSize.constantMin = math.lerp(farCloudSize.x, farCloudSize.y, cloud);
            farStartSize.constantMax = math.lerp(farCloudSize.z, farCloudSize.w, cloud);
            farMain.startSize = farStartSize;

            var nearStartSize = nearMain.startSize;
            nearStartSize.constantMin = math.lerp(nearCloudSize.x, nearCloudSize.y, cloud);
            nearStartSize.constantMax = math.lerp(nearCloudSize.z, nearCloudSize.w, cloud);
            nearMain.startSize = nearStartSize;
        }
    }
    
    float _cloud = 0;
    public void SetWeather(Weather weather,float lightningLight,bool immediatelyStop=false)
    { 
        if (weather.IsSnow())
        {
            weatherMono.SetRain(0, immediatelyStop);
            weatherMono.SetSnow(weather.waterFall, immediatelyStop);
        }
        else
        {
            weatherMono.SetSnow(0, immediatelyStop);
            weatherMono.SetRain(weather.waterFall, immediatelyStop);
        }
        weatherMono.SetFog(weather.fog, immediatelyStop);
        weatherMono.SetWind(weather.wind, immediatelyStop);
        float cloudValue = cloud+ weather.waterFall * 3;
         cloud = weather.cloud;
        /*if (cloudValue > weather.cloud)
        {
            cloud = cloudValue;
        }*/
        cloud = cloud > 1 ? 1 : cloud;

        float windValue = weather.wind;
        windValue = EnvironmentManger.instance.nowWaterFall > 0 ? windValue * 0.5f : windValue;
        if (windValue < 0.2f && windValue > -0.2f)
        {
            if (windValue < 0)
            {
                windValue = -0.2f;
            }
            else
            {
                windValue = 0.2f;
            }
        }

        farVelocity.speedModifier = -windValue;
        nearVelocity.speedModifier = -windValue;
        Shader.SetGlobalFloat("_CloudValue", cloud * (1 - lightningLight));

        if (setWindValue != null)
        {
            setWindValue(weather.wind);
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