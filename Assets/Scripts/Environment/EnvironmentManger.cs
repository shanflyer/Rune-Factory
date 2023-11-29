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
}

public class EnvironmentManger : Singleton<EnvironmentManger>
{
    private Transform environmentParent;
    private Light2D directionLight;
    private Light2D globalLight;
    private Transform sunTransform;

    public override void Init()
    {
        base.Init();
        environmentParent = new GameObject("Environment").transform;

        GameObject sunPrefab = Resources.Load<GameObject>("Prefabs/Sun");
        if (sunPrefab)
        {
            sunTransform = GameObject.Instantiate(sunPrefab, environmentParent).transform;
        }

        GameObject DirectionLightGameObject = new GameObject("DirectionLight");
        directionLight = DirectionLightGameObject.AddComponent<Light2D>();
        directionLight.lightType = Light2D.LightType.Directional;
        directionLight.useNormalMap = true;
        directionLight.normalMapQuality = Light2D.NormalMapQuality.Fast;
        directionLight.SetShapePath(new Vector3[]
        {
            new Vector3(-100,-100),new Vector3(-100,100),new Vector3(100,100),new Vector3(100,-100)
        });
        DirectionLightGameObject.transform.SetParent(environmentParent, false);

        GameObject GlobalLightGameObject = new GameObject("GlobalLight");
        globalLight = GlobalLightGameObject.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;
        GlobalLightGameObject.transform.SetParent(environmentParent, false);

        GameObject.DontDestroyOnLoad(environmentParent.gameObject);

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
            sunTransform.localScale = new Vector3(natureLightData.sunScale, natureLightData.sunScale, 1);
            sunTransform.localPosition = natureLightData.sunPos;

            globalLight.color = natureLightData.globalColor;
            globalLight.intensity = natureLightData.globalIntensity;

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
        }
        if (OverrideEnvironmentLight.overSkyAndSun)
        {
            Shader.SetGlobalColor("_CloudColor", environmentLight.cloudColor);
            Shader.SetGlobalColor("_SkyTopColor", environmentLight.skyTopColor);
            Shader.SetGlobalColor("_SkyBottomColor", environmentLight.skyBottomColor);
            Shader.SetGlobalFloat("_SkyHalfValue", environmentLight.skyHalfValue);
            Shader.SetGlobalColor("_SunColor", environmentLight.sunColor);
            sunTransform.localScale = new Vector3(environmentLight.sunScale, environmentLight.sunScale, 1);
            sunTransform.localPosition = environmentLight.sunPos;
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
        }
        Shader.SetGlobalFloat("_ShadowValue", natureLightData.shadowValue);

        Shader.SetGlobalColor("_CloudColor", natureLightData.cloudColor);
        Shader.SetGlobalColor("_SkyTopColor", natureLightData.skyTopColor);
        Shader.SetGlobalColor("_SkyBottomColor", natureLightData.skyBottomColor);
        Shader.SetGlobalFloat("_SkyHalfValue", natureLightData.skyHalfValue);
        Shader.SetGlobalColor("_SunColor", natureLightData.sunColor);
        sunTransform.localScale = new Vector3(natureLightData.sunScale, natureLightData.sunScale, 1);
        sunTransform.localPosition = natureLightData.sunPos;
    }

    protected override void Clear()
    {
        base.Clear();
    }
}