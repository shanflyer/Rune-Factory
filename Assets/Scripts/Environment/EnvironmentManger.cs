using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public struct EnvironmentLightData
{
    public Color globalColor;
    public float globalIntensity;
    public Color color;
    public Vector3 direction;
    public float intensity;
}
public class EnvironmentManger:Singleton<EnvironmentManger>
{
    Transform environmentParent;
    Light2D directionLight;
    Light2D globalLight;
    public override void Init()
    {
        base.Init();
        environmentParent = new GameObject("Environment").transform;

        GameObject DirectionLightGameObject = new GameObject("DirectionLight");
        directionLight = DirectionLightGameObject.AddComponent<Light2D>();
        directionLight.lightType = Light2D.LightType.Directional;
        DirectionLightGameObject.transform.SetParent(environmentParent, false);

        GameObject GlobalLightGameObject = new GameObject("GlobalLight");
        globalLight = GlobalLightGameObject.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global; 
        GameObject.DontDestroyOnLoad(environmentParent.gameObject);

        GameActionManager.instance.AddListener<SetEnvironmentLight>(SetEnvironmentLight);
        GameActionManager.instance.AddListener<OverrideEnvironmentLight>(OverrideEnvironmentLight);
        GameActionManager.instance.AddListener<ClearOverrideEnvironmentLight>(ClearOverrideEnvironmentLight);
    }

    EnvironmentLightData natureLightData;
    bool overrideEnvironment; 

    void SetEnvironmentLight(SetEnvironmentLight SetEnvironmentLight)
    {
        natureLightData = SetEnvironmentLight.environmentLightData;

        if (!overrideEnvironment)
        {
            globalLight.color = natureLightData.globalColor;
            globalLight.intensity = natureLightData.globalIntensity;  

            directionLight.Direction = natureLightData.direction;
            directionLight.color = natureLightData.color;
            directionLight.intensity = natureLightData.intensity;
        } 
    }
     
    void OverrideEnvironmentLight(OverrideEnvironmentLight OverrideEnvironmentLight)
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
    }
    
    void ClearOverrideEnvironmentLight(ClearOverrideEnvironmentLight clearOverrideEnvironmentLight)
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
    }
    
    protected override void Clear()
    {
        base.Clear();
    }
}