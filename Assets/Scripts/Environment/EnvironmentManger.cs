using System.Collections.Generic;
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
    public int sunValue;
}

public class EnvironmentManger : Singleton<EnvironmentManger>
{
    SkyEnviromentMono skyEnviromentMono;
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

    public override void Init()
    {
        base.Init(); 
        if (skyEnviromentMono == null)
        {
            var _skyEnviromentMono = Resources.Load<SkyEnviromentMono>("Prefabs/Environment");
            skyEnviromentMono = GameObject.Instantiate(_skyEnviromentMono);
           // GameObject.DontDestroyOnLoad(skyEnviromentMono.gameObject);
        }
         
        GameActionManager.instance.AddListener<SetEnvironmentLight>(SetEnvironmentLight);
        GameActionManager.instance.AddListener<OverrideEnvironmentLight>(OverrideEnvironmentLight);
        GameActionManager.instance.AddListener<ClearOverrideEnvironmentLight>(ClearOverrideEnvironmentLight);

    }

    private EnvironmentLightData natureLightData;
    private bool overrideEnvironment;

    private void SetEnvironmentLight(SetEnvironmentLight SetEnvironmentLight)
    {
        natureLightData = SetEnvironmentLight.environmentLightData;
         
        if (!overrideEnvironment)
        {
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
            

            globalLight.color = natureLightData.globalColor;
            globalLight.intensity = natureLightData.globalIntensity;

            Shader.SetGlobalColor("_GlobalColor", globalLight.color * globalLight.intensity);

            directionLight.Direction = natureLightData.direction;
            directionLight.color = natureLightData.color;
            directionLight.intensity = natureLightData.intensity;
            Shader.SetGlobalFloat("_ShadowValue", natureLightData.shadowValue);
        }
    }

    private void OverrideEnvironmentLight(OverrideEnvironmentLight OverrideEnvironmentLight)
    {
        var environmentLight = OverrideEnvironmentLight.environmentLightData;
        overrideEnvironment = true;
        if (directionLight)
        {
            directionLight.Direction = environmentLight.direction;
            directionLight.color = environmentLight.color;
            directionLight.intensity = environmentLight.intensity;
        }
        if (globalLight)
        {
            globalLight.color = environmentLight.globalColor;
            globalLight.intensity = environmentLight.globalIntensity;
            Shader.SetGlobalColor("_GlobalColor", globalLight.color * globalLight.intensity);
        }
        if (OverrideEnvironmentLight.overSkyAndSun)
        {
            Shader.SetGlobalColor("_CloudColor", environmentLight.cloudColor);
            Shader.SetGlobalColor("_SkyTopColor", environmentLight.skyTopColor);
            Shader.SetGlobalColor("_SkyBottomColor", environmentLight.skyBottomColor);
            Shader.SetGlobalFloat("_SkyHalfValue", environmentLight.skyHalfValue);
            Shader.SetGlobalColor("_SunColor", environmentLight.sunColor);
            Shader.SetGlobalInt("_Sun", environmentLight.sunValue);
            if (sunTransform)
            {
                sunTransform.localScale = new Vector3(environmentLight.sunScale, environmentLight.sunScale, 1);
                sunTransform.localPosition = environmentLight.sunPos;
            }
                
        }
        Shader.SetGlobalFloat("_ShadowValue", environmentLight.shadowValue);
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
}