using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public struct EnvironmentLightData
{
    public Color globalColor;
    public Color cloudColor;
    public Color skyTopColor,skyBottomColor;
    public float skyHalfValue;
    public float globalIntensity;
    public Color color;
    public Vector3 direction;
    public float intensity;
    public float shadowValue;
    public Vector2 sunPos;
    public float sunScale;
    public Color sunColor;
    public Color flareColor;
    public int sunValue;
}

public class EnvironmentManger : Singleton<EnvironmentManger>
{
    public override bool NeedUpdata => true;
    SkyEnviromentMono skyEnviromentMono;
    public SkyEnviromentMono SkyEnviromentMono =>skyEnviromentMono;
    ProFlare flare => skyEnviromentMono.ProFlare;
    Transform sunTransform => skyEnviromentMono.Sun;
    Light2D globalLight=> skyEnviromentMono.GlobalLight;
    Light2D directionLight => skyEnviromentMono.DirectionLight;

    private Dictionary<int,MyLight> lights=new Dictionary<int, MyLight>();
    private List<int> lightIds =new List<int>();  
    public void AddMyLight(MyLight myLight)
    {
        int instanceID = myLight.GetInstanceID();
        if (!lightIds.Contains(instanceID))
        {
            lights.Add(instanceID, myLight);
            lightIds.Add(instanceID); 
        }
        myLight.LerpTimeValue(timeValue);

    }
    public void RemoveMyLight(MyLight myLight)
    {
        int instanceID = myLight.GetInstanceID();
        lights.Remove(instanceID);
        lightIds.Remove(instanceID );
    }

    float timeValue;
    public void UpDataMyLightTimeValue(float timeValue)
    {
        this.timeValue = timeValue;
        for(int i = 0; i < lightIds.Count; i++)
        {
            lights[lightIds[i]].LerpTimeValue(timeValue);
        }
    }

    public void SetCameraPos(Vector2 pos)
    {
        if(skyEnviromentMono)
            skyEnviromentMono.SetBgPos(pos);
    }
    public override async void Init()
    {
        base.Init(); 
        if (skyEnviromentMono == null)
        {
            var _skyEnviromentMono = Resources.Load<SkyEnviromentMono>("Prefabs/Environment");

            var asyncInstantiateOperation = GameObject.InstantiateAsync(_skyEnviromentMono, CameraManager.instance.mainCamera.transform);
            await asyncInstantiateOperation;
            skyEnviromentMono = asyncInstantiateOperation.Result[0];
            var skyPos = skyEnviromentMono.transform.position;
            var skyLocalPos = skyEnviromentMono.transform.localPosition;
            skyLocalPos.z-=skyPos.z;
            skyEnviromentMono.transform.localPosition = skyLocalPos;
            skyEnviromentMono.SetBgPos(CameraManager.instance.oldCameraPos);
           // GameObject.DontDestroyOnLoad(skyEnviromentMono.gameObject);
        }
         
        GameActionManager.instance.AddListener<SetEnvironmentLight>(SetEnvironmentLight);
        GameActionManager.instance.AddListener<OverrideEnvironmentLight>(OverrideEnvironmentLight);
        GameActionManager.instance.AddListener<ClearOverrideEnvironmentLight>(ClearOverrideEnvironmentLight);
        GameActionManager.instance.AddListener<SetWeather>(SetWeather);

    }

    private EnvironmentLightData natureLightData;
    private EnvironmentLightData overrideLightData;
    bool overSkyAndSun;
    private Weather nowWeather;
    private bool overrideEnvironment;

    private HashSet<WindEffect> windEffects = new HashSet<WindEffect>();
    public void AddWindEffect(WindEffect windEffect)
    {
        windEffects.Add(windEffect);
        windEffect.SetWindValue(nowWeather.wind);
    }
    public void RemoveWindEffect(WindEffect windEffect)
    {
        windEffects.Remove(windEffect);
    }

    void SetWeather(SetWeather setWeather)
    { 
        var lerp = LerpWeather(setWeather.weather);
        GameObjectCurveController.instance.StartIEnumerator(lerp); 
    }

    IEnumerator LerpWeather(Weather newWeather)
    {
        float timeValue = 0;
        bool oldDamp = !nowWeather.IsSnow() && nowWeather.waterFall > 0;
        bool newDamp= !newWeather.IsSnow() && newWeather.waterFall > 0;
        Weather weather = nowWeather;
        while (timeValue<=2)
        {
            float value = timeValue / 2.0f;
            nowWeather = Weather.Lerp(weather, newWeather, value);
            skyEnviromentMono.SetWeather(nowWeather);
            using(var e = windEffects.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    e.Current.SetWindValue(nowWeather.wind);
                }
            }
            float weatherLightValue= nowWeather.GetWeatherLight();
            float flareLight = nowWeather.GetFlareLight();
            RefreshEnvironment(weatherLightValue, flareLight);

            if (oldDamp && !newDamp)
            {
                Shader.SetGlobalFloat("_DampValue", 1 - timeValue);
            }else
            if (!oldDamp && newDamp)
            {
                Shader.SetGlobalFloat("_DampValue", timeValue);
            }else if (newDamp)
            {
                Shader.SetGlobalFloat("_DampValue", 1);
            }
            else
            {
                Shader.SetGlobalFloat("_DampValue", 0);
            }

            timeValue += Time.deltaTime;
            yield return 0;
        } 
        nowWeather = newWeather;
    }


    void RefreshEnvironment(float weatherLightValue, float flareLight)
    {  
        if (!overrideEnvironment)
        {
            Color cloudColor = natureLightData.cloudColor;
            float cloudColorA = cloudColor.a;
            cloudColor *= weatherLightValue;
            cloudColor.a = cloudColorA;

            Color skyTopColor = natureLightData.skyTopColor;
            float skyTopColorA = skyTopColor.a;
            skyTopColor *= weatherLightValue;
            skyTopColor.a = skyTopColorA;

            Color skyBottomColor = natureLightData.skyBottomColor;
            float skyBottomColorA = skyBottomColor.a;
            skyBottomColor *= weatherLightValue;
            skyBottomColor.a = skyBottomColorA;

            Color flareColor = natureLightData.flareColor; 
            flareColor *= weatherLightValue* flareLight;

            Color sunColor = natureLightData.sunColor;
            float sunColorA = sunColor.a;
            sunColor*= weatherLightValue;
            sunColor.a = sunColorA;

            Shader.SetGlobalColor("_CloudColor", cloudColor);
            Shader.SetGlobalColor("_SkyTopColor", skyTopColor);
            Shader.SetGlobalColor("_SkyBottomColor", skyBottomColor);
            Shader.SetGlobalFloat("_SkyHalfValue", natureLightData.skyHalfValue);
            Shader.SetGlobalColor("_SunColor", sunColor);
            Shader.SetGlobalInt("_Sun", natureLightData.sunValue);
            if (sunTransform)
            {
                sunTransform.localScale = new Vector3(natureLightData.sunScale, natureLightData.sunScale, 1);
                sunTransform.localPosition = natureLightData.sunPos;
            }
            flare.GlobalTintColor = flareColor;

            globalLight.color = natureLightData.globalColor;
            globalLight.intensity = natureLightData.globalIntensity;

            Shader.SetGlobalColor("_GlobalColor", globalLight.color * globalLight.intensity);

            directionLight.Direction = natureLightData.direction;
            directionLight.color = natureLightData.color;
            directionLight.intensity = natureLightData.intensity * weatherLightValue;

            Shader.SetGlobalColor("_DirectionColor", directionLight.color * directionLight.intensity);

            Shader.SetGlobalFloat("_ShadowValue", natureLightData.shadowValue);
        }
        else
        {
            if (directionLight)
            {
                directionLight.Direction = overrideLightData.direction;
                directionLight.color = overrideLightData.color;
                directionLight.intensity = overrideLightData.intensity * weatherLightValue;
            }
            if (globalLight)
            {
                globalLight.color = overrideLightData.globalColor;
                globalLight.intensity = overrideLightData.globalIntensity;
                Shader.SetGlobalColor("_GlobalColor", globalLight.color * globalLight.intensity);
            }
            if (overSkyAndSun)
            {
                Color cloudColor = overrideLightData.cloudColor;
                float cloudColorA = cloudColor.a;
                cloudColor *= weatherLightValue;
                cloudColor.a = cloudColorA;

                Color skyTopColor = overrideLightData.skyTopColor;
                float skyTopColorA = skyTopColor.a;
                skyTopColor *= weatherLightValue;
                skyTopColor.a = skyTopColorA;

                Color skyBottomColor = overrideLightData.skyBottomColor;
                float skyBottomColorA = skyBottomColor.a;
                skyBottomColor *= weatherLightValue;
                skyBottomColor.a = skyBottomColorA;

                Color flareColor = overrideLightData.flareColor;
                flareColor *= weatherLightValue * flareLight;



                Shader.SetGlobalColor("_CloudColor", cloudColor);
                Shader.SetGlobalColor("_SkyTopColor", skyTopColor);
                Shader.SetGlobalColor("_SkyBottomColor", skyBottomColor);
                Shader.SetGlobalFloat("_SkyHalfValue", overrideLightData.skyHalfValue);
                Shader.SetGlobalColor("_SunColor", overrideLightData.sunColor);
                Shader.SetGlobalInt("_Sun", overrideLightData.sunValue);
                if (sunTransform)
                {
                    sunTransform.localScale = new Vector3(overrideLightData.sunScale, overrideLightData.sunScale, 1);
                    sunTransform.localPosition = overrideLightData.sunPos;
                }
                flare.GlobalTintColor = flareColor;
            }
            Shader.SetGlobalFloat("_ShadowValue", overrideLightData.shadowValue);
        }
    }
    private void SetEnvironmentLight(SetEnvironmentLight SetEnvironmentLight)
    {
        natureLightData = SetEnvironmentLight.environmentLightData;
        RefreshEnvironment(nowWeather.GetWeatherLight(),nowWeather.GetFlareLight()); 
    }

    private void OverrideEnvironmentLight(OverrideEnvironmentLight OverrideEnvironmentLight)
    {
        overrideLightData = OverrideEnvironmentLight.environmentLightData;
        overrideEnvironment = true;
        overSkyAndSun = OverrideEnvironmentLight.overSkyAndSun;
        RefreshEnvironment(nowWeather.GetWeatherLight(), nowWeather.GetFlareLight());
    }

    private void ClearOverrideEnvironmentLight(ClearOverrideEnvironmentLight clearOverrideEnvironmentLight)
    {
        overrideEnvironment = false;
        if (directionLight)
        {
            directionLight.Direction = natureLightData.direction;
            directionLight.color = natureLightData.color;
            directionLight.intensity = natureLightData.intensity;
        }
        if (globalLight)
        {
            globalLight.color = natureLightData.globalColor;
            globalLight.intensity = natureLightData.globalIntensity;

            Shader.SetGlobalColor("_GlobalColor", globalLight.color * globalLight.intensity);
        }
        Shader.SetGlobalFloat("_ShadowValue", natureLightData.shadowValue);

        Shader.SetGlobalColor("_CloudColor", natureLightData.cloudColor);
        Shader.SetGlobalColor("_SkyTopColor", natureLightData.skyTopColor);
        Shader.SetGlobalColor("_SkyBottomColor", natureLightData.skyBottomColor);
        Shader.SetGlobalFloat("_SkyHalfValue", natureLightData.skyHalfValue);
        Shader.SetGlobalColor("_SunColor", natureLightData.sunColor);
        Shader.SetGlobalInt("_Sun", natureLightData.sunValue);
        if (sunTransform)
        {
            sunTransform.localScale = new Vector3(natureLightData.sunScale, natureLightData.sunScale, 1);
            sunTransform.localPosition = natureLightData.sunPos;
        } 
    }

    protected override void Clear()
    {
        base.Clear();
    } 
    protected override void UpData()
    {
         
        base.UpData();
    }
}