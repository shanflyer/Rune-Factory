using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public struct EnvironmentLightData
{
    public Color globalColor;
    public Color cloudColor;
    public Color skyTopColor,skyBottomColor;
    public float skyHalfValue; 
    public Color color;
    public Vector3 direction; 
    public float shadowValue;
    public Vector2 sunPos;
    public float sunScale;
    public Color sunColor;
    public Color flareColor;
    public int sunValue;
}
public struct CharacterFootStep
{
    public Transform transform; 
    public SetValue FootStepAction;
} 
public struct CharacterGetFootStep
{
    public Transform transform; 
    public SetFootStepAction SetFootStepAction;
}

public enum WeatherDisplayType
{
    Outside,Inside,Forest
}
public class EnvironmentManger : Singleton<EnvironmentManger>
{
    public override bool NeedUpdata => true;
    SkyEnviromentMono skyEnviromentMono;
    
    public SkyEnviromentMono SkyEnviromentMono =>skyEnviromentMono;
    ProFlare flare => skyEnviromentMono.ProFlare;
    Transform sunTransform => skyEnviromentMono.Sun;

    private Dictionary<int,MyLight> lights=new Dictionary<int, MyLight>();
    private List<int> lightIds =new List<int>();

    MyDic<int,Audio2DPolygon> audio2DPolygons = new MyDic<int,Audio2DPolygon>();

    public void AddAudio2DPolygon(Audio2DPolygon audio2DPolygon)
    {
        int instanceID = audio2DPolygon.GetInstanceID();
        audio2DPolygons.TrySetValue(instanceID, audio2DPolygon);
        try
        {
            audio2DPolygon.RefreshAudio(CharacterManager.instance.ControllerRuntimeObj.collider);
        }
        catch
        {
            audio2DPolygon.RefreshAudio(null);
        }
        
    }
    public void RemoveAudio2DPolygon(Audio2DPolygon audio2DPolygon)
    {
        int instanceID = audio2DPolygon.GetInstanceID();
        audio2DPolygons.Remove(instanceID); 
    }
    public void UpDataAudio2DPolygon()
    {
        for(int i = 0; i < audio2DPolygons.length; i++)
        {
            audio2DPolygons[i].RefreshAudio(CharacterManager.instance.ControllerRuntimeObj.collider);
        }
    }

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


    FootstepDataList FootstepDataList;
    public FootstepSource GetMapFootStepSource(int index)
    {
        bool isOutSide = WorldMapObjManager.instance.IsOutSideMap;
        bool isSnow = nowWeather.IsSnow();
        bool waterFall = nowWeather.waterFall > 0.2f;
        int3 key = new int3(isOutSide ? 1 : 0, index, !isSnow ? (waterFall ? 1 :0) : 2);
        return FootstepDataList.GetSource(key); 
    }
   
    Lightning lightning;
    public void GetNowFootStepData(CharacterGetFootStep characterGetFootStep,int2 coordinate,int mapInstance,int defaultGround)
    {
        int groundIndex = MapCellController.instance.GetGroundIndex(coordinate, mapInstance);
        if (groundIndex <= 0)
        {
            groundIndex = defaultGround;
        }
        FootstepSource footstepSource = GetMapFootStepSource(groundIndex);
        if (footstepSource.clips != null)
        {
            AudioClip audioClip = footstepSource.clips[GameRandom.RandomInt(0, footstepSource.clips.Count)];
            characterGetFootStep.SetFootStepAction(audioClip, footstepSource.footStepColor);
        }
    }

    public void AddCharacterGetFootStep(int instanceId,CharacterGetFootStep characterGetFootStep)
    {
        if (!CharacterFootStepDic.ContainsKey(instanceId))
        {
            CharacterFootStep characterFootStep = new CharacterFootStep
            {
                transform = characterGetFootStep.transform,
                FootStepAction = (int index) =>
                    {
                        // Debug.Log("foot result:" + index);
                        try
                        {
                            FootstepSource footstepSource = GetMapFootStepSource(index);
                            if (footstepSource.clips != null)
                            {
                                AudioClip audioClip = footstepSource.clips[GameRandom.RandomInt(0, footstepSource.clips.Count)];
                                characterGetFootStep.SetFootStepAction(audioClip, footstepSource.footStepColor);
                            }
                        }
                        catch { } 
                    },
            };
            CharacterFootStepDic.Add(instanceId, characterFootStep);
        }
    }
    public void RemoveCharacterGetFootStep(int instanceId)
    {
        CharacterFootStepDic.Remove(instanceId);
    }

    MyDic<int, CharacterFootStep> CharacterFootStepDic = new MyDic<int, CharacterFootStep>();

     
    public List<CharacterFootStep> GetCharacterFootSteps()
    {
       return CharacterFootStepDic.GetValueList(true);
    } 
 
    public override async void Init()
    {
        base.Init();
        FootstepDataList=await GameDataManager.instance.GetAsyncData<FootstepDataList>();

        if (lightning == null)
        {
            lightning = new Lightning();
            lightning.setLightningLight = SetLightningValue;
        }

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

    public float lightningLight => lightning==null?0: lightning.lightningLight;
    public float weatherLight => nowWeather.GetWeatherLight();

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

    WeatherDisplayType weatherDisplayType;
    public void ChangeWeatherDisplayType(WeatherDisplayType weatherDisplayType)
    {
        this.weatherDisplayType = weatherDisplayType;
        
        if (weatherDisplayType == WeatherDisplayType.Inside)
        {
            Shader.SetGlobalFloat("_DampValue", 0); 
        }
        else
        { 
            Shader.SetGlobalFloat("_DampValue", nowWeather.waterFall > 0 && !nowWeather.IsSnow() ? 1 : 0);
        }
        skyEnviromentMono.ChangeWeatherDisplayType(weatherDisplayType);
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
            lightning.lightning = nowWeather.lightning;
            skyEnviromentMono.SetWeather(nowWeather, lightning.lightningLight);
            using (var e = windEffects.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    e.Current.SetWindValue(nowWeather.wind);
                }
            }

            if (oldDamp && !newDamp)
            {
                Shader.SetGlobalFloat("_DampValue", 1 - timeValue);
            }
            else if (!oldDamp && newDamp)
            {
                Shader.SetGlobalFloat("_DampValue", timeValue);
            }
            else if (newDamp)
            {
                Shader.SetGlobalFloat("_DampValue", 1);
            }
            else
            {
                Shader.SetGlobalFloat("_DampValue", 0);
            }

            float weatherLightValue= nowWeather.GetWeatherLight();
            float flareLight = nowWeather.GetFlareLight();
            RefreshEnvironment(weatherLightValue, flareLight); 
            timeValue += Time.deltaTime;
            yield return 0;
        } 
        nowWeather = newWeather;
    }


    public void SetLightningValue(float value)
    {
        float weatherLightValue = nowWeather.GetWeatherLight();
        float flareLight = nowWeather.GetFlareLight();
        RefreshEnvironment(weatherLightValue, flareLight);
        if (weatherDisplayType!=WeatherDisplayType.Inside)
        {
            skyEnviromentMono.SetWeather(nowWeather, lightning.lightningLight);
        }
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
             

            Shader.SetGlobalColor("_GlobalColor", natureLightData.globalColor);
             
            Shader.SetGlobalColor("_DirectionColor", Color.Lerp(natureLightData.color, lightning.lightningColor, lightning.lightningLight) * weatherLightValue);
            Shader.SetGlobalVector("_Direction", natureLightData.direction);
            Shader.SetGlobalFloat("_ShadowValue", natureLightData.shadowValue+ lightning.lightningLight*0.5f);
        }
        else
        {
            Color directionColor = Color.Lerp(overrideLightData.color, lightning.lightningColor, lightning.lightningLight); 
            Shader.SetGlobalVector("_Direction", overrideLightData.direction);
            Shader.SetGlobalVector("_DirectionColor", directionColor * weatherLightValue);
            Shader.SetGlobalColor("_GlobalColor", overrideLightData.globalColor);
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
            Shader.SetGlobalFloat("_ShadowValue", overrideLightData.shadowValue + lightning.lightningLight * 0.5f);
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
        
        Shader.SetGlobalVector("_Direction", natureLightData.direction);
        Shader.SetGlobalVector("_DirectionColor", natureLightData.color);
        Shader.SetGlobalColor("_GlobalColor", natureLightData.globalColor);

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
        CharacterFootStepDic.Clear();
        base.Clear();
    }

    internal class Lightning
    {
        internal float lightningLight;
        internal Color lightningColor=>LightningData.color;
        internal SetFloatValue setLightningLight;
        public Lightning()
        {
            lightningLight = 0;
            InitDataAsync();
        }
        async Task InitDataAsync()
        {
            LightningData = await GameSourceManager.instance.GetSingleScriptableObject<LightningData>("Data/LightningData");
        }

        void LightningAction()
        {
           
            float lightningTime = GameRandom.RandomFloat(LightningData.lightningSpeed) * LightningData.LightningTime;
            float waitSoundTime = (1 - lightning) * GameRandom.RandomFloat(LightningData.waitSoundTime)+ lightningTime; 
            GameObjectCurveController.instance.StartIEnumerator(Lightning());

            IEnumerator Lightning()
            {
                float timeValue = 0;
                while (timeValue<= waitSoundTime)
                {
                    if(timeValue<= lightningTime)
                    {
                        lightningLight = LightningData.lightningCurve.Evaluate(timeValue / lightningTime) * lightning;
                        if (setLightningLight != null)
                        {
                            setLightningLight(lightningLight);
                        }
                    } 
                    timeValue += Time.deltaTime;
                    yield return 0;
                }
                lightningLight = 0;
                setLightningLight(lightningLight);
                if (LightningData.sounds!=null&&LightningData.sounds.Length>0)
                {
                    int index = GameRandom.RandomInt(0, LightningData.sounds.Length);
                    AudioController.instance.PlayAudio(LightningData.sounds[index],audioClearType:AudioClearType.All,Group: BGSGroup.Lightning.ToString());
                }
                LightningCD = 0;
                waitLightningTime = 0;

            }
        } 
        LightningData LightningData;
        float waitLightningTime = 0;
        float LightningCD = 0;
        internal float lightning;
        internal void UpData()
        { 
            if (lightning > 0)
            {
                if (LightningCD == 0)
                {
                    LightningCD = GameRandom.RandomFloat(LightningData.lightningCD);
                }
                if (waitLightningTime < LightningCD)
                {
                    waitLightningTime += Time.deltaTime;
                    if (waitLightningTime >= LightningCD)
                    {
                        LightningAction();
                    }
                } 
            } 
        }
    }

   
    protected override void UpData()
    { 
        if (lightning != null)
        {
            lightning.UpData();
        }
            base.UpData();
    }
}