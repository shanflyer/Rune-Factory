using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.ParticleSystem;

 
public class GameVolumeManager : Singleton<GameVolumeManager>
{
    public override bool NeedUpdate => true;
    Material screenMat;
    // Unity 6 使用 EntityId 作为对象键，避免继续依赖已废弃的 int InstanceID。
    MyDic<EntityId, GameVolumeObject> volumeObjects = new MyDic<EntityId, GameVolumeObject>();
    Dictionary<string, ParticleSystem> singleParticleDic = new Dictionary<string, ParticleSystem>();
    private Task initializationTask = Task.CompletedTask;
    public override Task InitializationTask => initializationTask;
    public override System.Collections.Generic.IReadOnlyList<System.Type> InitializationDependencies => new[] { typeof(GameActionManager) };

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async Task InitAsync()
    {
        screenMat = await ExtensionsResources.LoadResourceAsync<Material>("Material/ScreenCycle");
        if (screenMat == null)
        {
            Debug.LogError("GameVolumeManager init failed: missing Material/ScreenCycle");
            return;
        }

        int width = Screen.width;
        int heigh = Screen.height;
        volumeLevel = PlayerPrefs.GetInt("VolumeLevel", 1);
        DepthFieldValue = PlayerPrefs.GetFloat("DepthField", 1);

        screenMat.SetFloat("_CycleSize", width > heigh ? width : heigh);
        GameActionManager.instance.AddListener<LerpScreenCycleValue>(LerpScreenCycleValue);
        GameActionManager.instance.AddListener<SetPlayerShaderPos>(SetPlayerShaderPos);

        singleParticleDic.Clear();
        var particleParent= GameController.instance.transform.Find("SingleParticle");
        for(int i = 0; i < particleParent.childCount; i++)
        {
            if(particleParent.GetChild(i).TryGetComponent(out ParticleSystem particleSystem))
            {
                singleParticleDic.Add(particleSystem.name, particleSystem);
                if (particleSystem.name == "脚印")
                {
                    footStep = particleSystem;
                }
            }
        }
    }
    private int _volumeLevel;
    public int volumeLevel
    {
        set
        {
            _volumeLevel = value;
            for(int i = 0; i < volumeObjects.length; i++)
            {
                volumeObjects[i].SetVolumeLevel(value);
            }
            PlayerPrefs.SetInt("VolumeLevel", value);
            switch (value)
            {
                case 0:
                    ScalableBufferManager.ResizeBuffers(0.5f, 0.5f);
                    break;
                case 1:
                    ScalableBufferManager.ResizeBuffers(0.75f, 0.75f);
                    break;
                case 2:
                    ScalableBufferManager.ResizeBuffers(1f, 1f);
                    break;
            }
            CameraManager.instance.SetVolumeLevel(value);
        }
        get
        {
            return _volumeLevel;
        }
    }
    private float depthFieldValue = 1.0f;
    public float DepthFieldValue
    {
        get { return depthFieldValue; } 
        set 
        { 
            depthFieldValue = value;
            PlayerPrefs.SetFloat("DepthField", depthFieldValue);
            CameraManager.instance.RefreshDepthOfField();
        }
    }

    private ParticleSystem footStep;
    public void EmitFootParticle(float angle,Vector3 pos,Color footStepColor,bool isLeftFoot)
    { 
        pos.z = 0;
        if (!float.IsFinite(pos.x) || !float.IsFinite(pos.y) || !float.IsFinite(angle))
        {
            return;
        } 
        EmitParams ep = new EmitParams();
        ep.startColor = footStepColor;
        ep.position = pos;
        ep.startSize = isLeftFoot ? footStep.main.startSize.constant : -footStep.main.startSize.constant;
        ep.rotation = 180 - angle;
        //Debug.Log($"ep.position{pos}");
        footStep.Emit(ep, 1);
    }
    public void ClearFootStep()
    {
        footStep.Clear();
    }
    private void SetPlayerShaderPos(SetPlayerShaderPos SetPlayerShaderPos)
    {
        Shader.SetGlobalVector("_PlayerPos", SetPlayerShaderPos.pos);
    }
    public void AddVolumeObject(GameVolumeObject volumeObject)
    {
        EntityId instanceID = volumeObject.GetEntityId();
        volumeObjects.Add(instanceID, volumeObject);
        volumeObject.SetVolumeLevel(volumeLevel);

    }
    public void RemoveVolumeObject(GameVolumeObject volumeObject)
    {
        EntityId instanceID = volumeObject.GetEntityId();
        volumeObjects.Remove(instanceID);
    }
    protected override void Clear()
    {
        base.Clear();
        initializationTask = Task.CompletedTask;
        if (screenMat != null)
        {
            screenMat.SetVector("_Offset", new Vector2(0.5f,0.5f));
            SetScreenCycleValue(0);
        }
    }
    void LerpScreenCycleValue(LerpScreenCycleValue LerpScreenCycleValue)
    {
        var screenPos = CameraManager.instance.mainCamera.WorldToScreenPoint(LerpScreenCycleValue.cyclePos);
        var screenSize = GameCommon.GetScreenResolution();
        Vector2 cyclePos = new Vector2(screenPos.x / screenSize.x, screenPos.y / screenSize.y);
        cyclePos.x = math.clamp(cyclePos.x, 0, 1);
        cyclePos.y = math.clamp(cyclePos.y, 0, 1);
        screenMat.SetVector("_Offset", cyclePos); 
       // Debug.Log($"screenPos:{screenPos}--screenSize:{screenSize}-LerpScreenCycleValue.cyclePos:{LerpScreenCycleValue.cyclePos}--{cyclePos}");
        float timeValue = 0;
        float minCycleValue = LerpScreenCycleValue.minCycleValue;
        float maxCycleValue = LerpScreenCycleValue.maxCycleValue;
        float lerpTime = LerpScreenCycleValue.lerpTime;


        if (minCycleValue == 0)
        {
            GameActionManager.instance.QueueAction(new HidePanelGroup { hide = true }, true);
        }

        GameObjectCurveController.instance.StartFrameTask((float deltaTime) =>
        {
            float value = math.lerp(minCycleValue, maxCycleValue, timeValue / lerpTime);
            SetScreenCycleValue(value);
            timeValue += deltaTime;
            return timeValue <= lerpTime;
        }, () =>
        {
        SetScreenCycleValue(maxCycleValue);
        if (LerpScreenCycleValue.setResult != null)
        {
            LerpScreenCycleValue.setResult(true);
        }

        if (maxCycleValue == 0)
        {
            GameActionManager.instance.QueueAction(new HidePanelGroup { hide = false }, true);
        }
        });
    }

    public void SetScreenCycleValue(float value)
    {
        screenMat.SetFloat("_CycleValue", value);
    }

  
    protected override void Update()
    { 
        base.Update();
    }
}
