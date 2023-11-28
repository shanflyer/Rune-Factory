using UnityEngine;
using UnityEngine.Rendering.Universal;

public struct EnvironmentLightData
{
    public Color globalColor;
    public Color cloudColor;
    public Color skyTopColor,skyBottomColor;
    public float globalIntensity;
    public Color color;
    public Vector3 direction;
    public float intensity;
    public float shadowValue;
}

public class EnvironmentManger : Singleton<EnvironmentManger>
{
    private Transform environmentParent;
    private Light2D directionLight;
    private Light2D globalLight;

    public override void Init()
    {
        base.Init();
        environmentParent = new GameObject("Environment").transform;

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
        Shader.SetGlobalColor("_CloudColor", natureLightData.cloudColor);
        Shader.SetGlobalColor("_SkyTopColor", natureLightData.skyTopColor);
        Shader.SetGlobalColor("_SkyBottomColor", natureLightData.skyBottomColor);

        if (!overrideEnvironment)
        {
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
    }

    protected override void Clear()
    {
        base.Clear();
    }
}