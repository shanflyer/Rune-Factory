using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using static EnvironmentManger;
using Unity.Mathematics;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;


public class GlobalShaderSetMono : MonoBehaviour
{
    public List<GlobalData> GlobalDatas = new List<GlobalData>();
    public void SetGlobalShaderValue()
    {
        for(int i=0;i<GlobalDatas.Count; i++)
        {
            GlobalDatas[i].InitGlobalShader();
        }
    }
    private void Awake()
    {
        SetGlobalShaderValue();
    }
    public Selectable selectable;
    public float dayValue;
    public EnvironmentDataList environmentDataList;
    public string dayName, nightName, dawnName, duskName;
    private EnvironmentData dayEnvironmentData, nightEnvironmentData;
    private EnvironmentData dawnEnvironmentData, duskEnvironmentData;
    public void TestLightValue()
    {
        dayEnvironmentData=environmentDataList.DataList.ToList().Find(e=>e.name==dayName);
        nightEnvironmentData = environmentDataList.DataList.ToList().Find(e => e.name == nightName);
        dawnEnvironmentData = environmentDataList.DataList.ToList().Find(e => e.name == dawnName);
        duskEnvironmentData = environmentDataList.DataList.ToList().Find(e => e.name == duskName);

        float dawnStart=0.23f;
        float dawnEnd=0.27f;
        float dayEnd =0.73f;
        float duskEnd = 0.77f;

        if (dayValue >= dawnStart && dayValue <= dawnEnd)
        {
            float lightValue = (float)(dayValue - dawnStart) / 0.042f;
            var environmentLightData = new EnvironmentLightData();
            environmentLightData.cloudColor = dawnEnvironmentData.CloudColor.Evaluate(lightValue);
            environmentLightData.skyTopColor = dawnEnvironmentData.SkyTopColor.Evaluate(lightValue);
            environmentLightData.skyBottomColor = dawnEnvironmentData.SkyBottomColor.Evaluate(lightValue);
            environmentLightData.globalColor = dawnEnvironmentData.GlobalColor.Evaluate(lightValue);
            environmentLightData.color = dawnEnvironmentData.Color.Evaluate(lightValue);
            environmentLightData.direction = new Vector3(dawnEnvironmentData.directionXValue.Evaluate(lightValue),
                          dawnEnvironmentData.directionYValue.Evaluate(lightValue), 0);
            environmentLightData.shadowValue = dawnEnvironmentData.shadowValue.Evaluate(lightValue);

            environmentLightData.skyHalfValue = dawnEnvironmentData.SkyHalfValue.Evaluate(lightValue);
            environmentLightData.skyTopColor = dawnEnvironmentData.SkyTopColor.Evaluate(lightValue);
            environmentLightData.skyBottomColor = dawnEnvironmentData.SkyBottomColor.Evaluate(lightValue);
            Color sunColor = dawnEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor *= dawnEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;
            environmentLightData.sunColor = sunColor;
            environmentLightData.sunScale = dawnEnvironmentData.sunScaleValue.Evaluate(lightValue);
            environmentLightData.sunPos = new Vector2(dawnEnvironmentData.sunXValue.Evaluate(lightValue),
                dawnEnvironmentData.sunYValue.Evaluate(lightValue));
            environmentLightData.cloudColor = dawnEnvironmentData.CloudColor.Evaluate(lightValue);
            environmentLightData.sunValue = 1;
            environmentLightData.flareColor = dawnEnvironmentData.flareColor.Evaluate(lightValue);

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = environmentLightData
            };
            RefreshEnvironment(setEnvironmentLight.environmentLightData);
        }
        else if (dayValue > dawnEnd && dayValue <= dayEnd)
        {
            float sunValue = (dayValue - dawnEnd) / 0.458f;

            Color sunColor = dayEnvironmentData.sunColor.Evaluate(sunValue);
            float a = sunColor.a;
            sunColor *= dayEnvironmentData.sunColorValue.Evaluate(sunValue);
            sunColor.a = a;
            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                {
                    globalColor = dayEnvironmentData.GlobalColor.Evaluate(sunValue),
                    color = dayEnvironmentData.Color.Evaluate(sunValue),
                    direction = new Vector3(dayEnvironmentData.directionXValue.Evaluate(sunValue),
                    dayEnvironmentData.directionYValue.Evaluate(sunValue), 0),
                    shadowValue = dayEnvironmentData.shadowValue.Evaluate(sunValue),

                    skyHalfValue = dayEnvironmentData.SkyHalfValue.Evaluate(sunValue),
                    cloudColor = dayEnvironmentData.CloudColor.Evaluate(sunValue),
                    skyTopColor = dayEnvironmentData.SkyTopColor.Evaluate(sunValue),
                    skyBottomColor = dayEnvironmentData.SkyBottomColor.Evaluate(sunValue),
                    sunColor = sunColor,
                    sunScale = dayEnvironmentData.sunScaleValue.Evaluate(sunValue),
                    sunPos = new Vector2(dayEnvironmentData.sunXValue.Evaluate(sunValue),
                      dayEnvironmentData.sunYValue.Evaluate(sunValue)),
                    sunValue = 1,
                    flareColor = dayEnvironmentData.flareColor.Evaluate(sunValue)
                }
            };
            RefreshEnvironment(setEnvironmentLight.environmentLightData);
        }
        else if (dayValue > dayEnd && dayValue < duskEnd)
        {
            float lightValue = (dayValue - dayEnd) / 0.042f;

            Color sunColor = duskEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor *= duskEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                {
                    globalColor = duskEnvironmentData.GlobalColor.Evaluate(lightValue),
                    color = duskEnvironmentData.Color.Evaluate(lightValue),
                    direction = new Vector3(duskEnvironmentData.directionXValue.Evaluate(lightValue),
                               duskEnvironmentData.directionYValue.Evaluate(lightValue), 0),
                    shadowValue = duskEnvironmentData.shadowValue.Evaluate(lightValue),

                    skyHalfValue = duskEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                    cloudColor = duskEnvironmentData.CloudColor.Evaluate(lightValue),
                    skyTopColor = duskEnvironmentData.SkyTopColor.Evaluate(lightValue),
                    skyBottomColor = duskEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                    sunColor = sunColor,
                    sunScale = duskEnvironmentData.sunScaleValue.Evaluate(lightValue),
                    sunPos = new Vector2(duskEnvironmentData.sunXValue.Evaluate(lightValue),
                      duskEnvironmentData.sunYValue.Evaluate(lightValue)),
                    sunValue = 1,
                    flareColor = duskEnvironmentData.flareColor.Evaluate(lightValue)
                }
            };
            RefreshEnvironment(setEnvironmentLight.environmentLightData);
        }
        else if (dayValue > dawnStart)
        {
            float lightValue = (float)(dayValue- dawnStart) / 0.23f;

            Color sunColor = nightEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor *= nightEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                {
                    globalColor = nightEnvironmentData.GlobalColor.Evaluate(lightValue),
                    color = nightEnvironmentData.Color.Evaluate(lightValue),
                    direction = new Vector3(nightEnvironmentData.directionXValue.Evaluate(lightValue),
                               nightEnvironmentData.directionYValue.Evaluate(lightValue), 0),
                    shadowValue = nightEnvironmentData.shadowValue.Evaluate(lightValue),

                    skyHalfValue = nightEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                    cloudColor = nightEnvironmentData.CloudColor.Evaluate(lightValue),
                    skyTopColor = nightEnvironmentData.SkyTopColor.Evaluate(lightValue),
                    skyBottomColor = nightEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                    sunColor = sunColor,
                    sunScale = nightEnvironmentData.sunScaleValue.Evaluate(lightValue),
                    sunPos = new Vector2(nightEnvironmentData.sunXValue.Evaluate(lightValue),
                      nightEnvironmentData.sunYValue.Evaluate(lightValue)),
                    sunValue = 0,
                    flareColor = nightEnvironmentData.flareColor.Evaluate(lightValue)
                }
            };
            RefreshEnvironment(setEnvironmentLight.environmentLightData);
        }
        else
        {
            float lightValue = (float)(1 - (dawnStart - dayValue)) / 0.23f;

            Color sunColor = nightEnvironmentData.sunColor.Evaluate(lightValue);
            float a = sunColor.a;
            sunColor *= nightEnvironmentData.sunColorValue.Evaluate(lightValue);
            sunColor.a = a;

            SetEnvironmentLight setEnvironmentLight = new SetEnvironmentLight
            {
                environmentLightData = new EnvironmentLightData
                {
                    globalColor = nightEnvironmentData.GlobalColor.Evaluate(lightValue),
                    color = nightEnvironmentData.Color.Evaluate(lightValue),
                    direction = new Vector3(nightEnvironmentData.directionXValue.Evaluate(lightValue),
                               nightEnvironmentData.directionYValue.Evaluate(lightValue), 0),
                    shadowValue = nightEnvironmentData.shadowValue.Evaluate(lightValue),

                    skyHalfValue = nightEnvironmentData.SkyHalfValue.Evaluate(lightValue),
                    cloudColor = nightEnvironmentData.CloudColor.Evaluate(lightValue),
                    skyTopColor = nightEnvironmentData.SkyTopColor.Evaluate(lightValue),
                    skyBottomColor = nightEnvironmentData.SkyBottomColor.Evaluate(lightValue),
                    sunColor = sunColor,
                    sunScale = nightEnvironmentData.sunScaleValue.Evaluate(lightValue),
                    sunPos = new Vector2(nightEnvironmentData.sunXValue.Evaluate(lightValue),
                      nightEnvironmentData.sunYValue.Evaluate(lightValue)),
                    sunValue = 0,
                    flareColor = nightEnvironmentData.flareColor.Evaluate(lightValue)
                }
            };

            RefreshEnvironment(setEnvironmentLight.environmentLightData);
        }

        void RefreshEnvironment(EnvironmentLightData natureLightData)
        {
            Color cloudColor = natureLightData.cloudColor;
            float cloudColorA = cloudColor.a; 
            cloudColor.a = cloudColorA;

            Color skyTopColor = natureLightData.skyTopColor;
            float skyTopColorA = skyTopColor.a; 
            skyTopColor.a = skyTopColorA;

            Color skyBottomColor = natureLightData.skyBottomColor;
            float skyBottomColorA = skyBottomColor.a; 
            skyBottomColor.a = skyBottomColorA;

            Color flareColor = natureLightData.flareColor; 

            Color sunColor = natureLightData.sunColor;
            float sunColorA = sunColor.a; 
            sunColor.a = sunColorA;

            Shader.SetGlobalColor("_CloudColor", cloudColor);
            Shader.SetGlobalColor("_SkyTopColor", skyTopColor);
            Shader.SetGlobalColor("_SkyBottomColor", skyBottomColor);
            Shader.SetGlobalFloat("_SkyHalfValue", natureLightData.skyHalfValue);
            Shader.SetGlobalColor("_SunColor", sunColor);
            Shader.SetGlobalInt("_Sun", natureLightData.sunValue);
           


            Shader.SetGlobalColor("_GlobalColor", natureLightData.globalColor);

            Shader.SetGlobalColor("_DirectionColor", natureLightData.color);
            Shader.SetGlobalVector("_Direction", natureLightData.direction);
            Vector2 directionValue = new Vector2(-natureLightData.direction.x * math.PI * 0.5f, 1 - natureLightData.direction.y);
            Shader.SetGlobalVector("LightDirection", directionValue);
            Shader.SetGlobalFloat("_ShadowValue", natureLightData.shadowValue );

            var MySpriteShadows = FindObjectsByType<MySpriteShadow>(FindObjectsSortMode.InstanceID);
            for (int i = 0; i < MySpriteShadows.Length; i++)
            {
                MySpriteShadows[i].SetDirectionAngle(natureLightData.direction.x );
            }
            if (skyEnviromentMono == null)
            {
                skyEnviromentMono = FindFirstObjectByType<SkyEnviromentMono>();
            }
            if (skyEnviromentMono != null)
            {
                var ScreenResolution = GameCommon.GetScreenResolution();
                float screenScale = ScreenResolution.x / ScreenResolution.y;
                float screenScaleX = 1f + screenScale; 
                float screenScaleY = 1f + screenScale * 0.5f;
                Vector2 sunPos = natureLightData.sunPos * new Vector2(screenScaleX, screenScaleY);
                if (skyEnviromentMono.Sun)
                {

                    skyEnviromentMono.Sun.localScale = new Vector3(natureLightData.sunScale, natureLightData.sunScale, 1);
                    skyEnviromentMono.Sun.localPosition = sunPos;
                } 
                Shader.SetGlobalVector("_SunPos", skyEnviromentMono.Sun.position); 

            }
        }

        var MyLights = FindObjectsByType<MyLight>(FindObjectsInactive.Include,FindObjectsSortMode.InstanceID);
        for(int i = 0; i < MyLights.Length; i++)
        {
            MyLights[i].Display(dayValue);
        }

        
    }
    SkyEnviromentMono skyEnviromentMono;
    public void TestGUID()
    {
        byte[] buffer=Guid.NewGuid().ToByteArray();
        var value0=BitConverter.ToInt64(buffer, 0);
        var value1 = BitConverter.ToInt32(buffer,11);
        Debug.Log($"value0:{value0}--value1{value1}");
    }


    Plane[] planes;
    [SerializeField]
    SpriteRenderer spriteRenderer;
    public void TestCameraRect()
    {
        planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        bool result = GeometryUtility.TestPlanesAABB(planes, spriteRenderer.bounds);
        Debug.Log($"TestPlanesAABB:{result}");

    }
}
#endif
public enum ShaderDataType
{
    INT,FLOAT,VECTOR,COLOR
}
[Serializable]
public struct GlobalData
{
    public string dataName;
    public ShaderDataType shaderDataType;
    public int intValue;
    public float floatValue;
    public Vector4 vectorValue;
    public Color colorValue;

    public void InitGlobalShader()
    {
        switch (shaderDataType)
        {
            case ShaderDataType.INT:
                Shader.SetGlobalInt(dataName, intValue);
                break;
            case ShaderDataType.FLOAT:
                Shader.SetGlobalFloat(dataName, floatValue);
                break;
            case ShaderDataType.VECTOR:
                Shader.SetGlobalVector(dataName, vectorValue);
                break;
            case ShaderDataType.COLOR:
                Shader.SetGlobalColor(dataName, colorValue* floatValue);
                break;
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(GlobalShaderSetMono))]
public class GlobalShaderSetMonoEditor : Editor
{
    public GlobalShaderSetMono globalShaderSetMono
    {
        get
        {
            return target as GlobalShaderSetMono;
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("测试GUID"))
        {
            globalShaderSetMono.TestGUID();
        }
        if (GUILayout.Button("设置"))
        {
            globalShaderSetMono.SetGlobalShaderValue();
        }
        if (GUILayout.Button("设置lightValue"))
        {
            globalShaderSetMono.TestLightValue();
        }
        if (GUILayout.Button("测试相机"))
        {
            globalShaderSetMono.TestCameraRect();
        }
    }
}
#endif