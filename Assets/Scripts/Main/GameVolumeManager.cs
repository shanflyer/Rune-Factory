using System.Collections;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GameVolumeManager : Singleton<GameVolumeManager>
{
    public override bool NeedUpdata => true;
    Material screenMat;
    MyDic<int, GameVolumeObject> volumeObjects = new MyDic<int, GameVolumeObject>();
    public override async void Init()
    {
        base.Init(); 
        screenMat = await ExtensionsResources.LoadResourceAsync<Material>("Material/ScreenCycle");
        int width = Screen.width;
        int heigh = Screen.height;
        volumeLevel = PlayerPrefs.GetInt("VolumeLevel", 1);

        screenMat.SetFloat("_CycleSize", width > heigh ? width : heigh);
        GameActionManager.instance.AddListener<LerpScreenCycleValue>(LerpScreenCycleValue);
        GameActionManager.instance.AddListener<SetPlayerShaderPos>(SetPlayerShaderPos);
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
        }
        get
        {
            return _volumeLevel;
        }
    }

    private void SetPlayerShaderPos(SetPlayerShaderPos SetPlayerShaderPos)
    {
        Shader.SetGlobalVector("_PlayerPos", SetPlayerShaderPos.pos);
    }
    public void AddVolumeObject(GameVolumeObject volumeObject)
    {
        int instanceID = volumeObject.GetInstanceID();
        volumeObjects.Add(instanceID, volumeObject);
        volumeObject.SetVolumeLevel(volumeLevel);

    }
    public void RemoveVolumeObject(GameVolumeObject volumeObject)
    {
        int instanceID = volumeObject.GetInstanceID();
        volumeObjects.Remove(instanceID);
    }
    protected override void Clear()
    {
        base.Clear();
        screenMat.SetVector("_Offset", new Vector2(0.5f,0.5f));
        SetScreenCycleValue(0);
    }
    void LerpScreenCycleValue(LerpScreenCycleValue LerpScreenCycleValue)
    {
        IEnumerator enumerator = LerpCycleValue(LerpScreenCycleValue);
        GameObjectCurveController.instance.StartIEnumerator(enumerator);
    }
    IEnumerator LerpCycleValue(LerpScreenCycleValue LerpScreenCycleValue)
    {
        var screenPos = CameraManager.instance.mainCamera.WorldToScreenPoint(LerpScreenCycleValue.cyclePos);
        var screenSize = GameCommon.GetScreenResolution();
        Vector2 cyclePos = new Vector2(screenPos.x / screenSize.x, screenPos.y / screenSize.y);
        screenMat.SetVector("_Offset", cyclePos); 
       // Debug.Log($"screenPos:{screenPos}--screenSize:{screenSize}-LerpScreenCycleValue.cyclePos:{LerpScreenCycleValue.cyclePos}--{cyclePos}");
        float timeValue = 0;
        var waitTime= new WaitForFixedUpdate();
        float minCycleValue = LerpScreenCycleValue.minCycleValue;
        float maxCycleValue = LerpScreenCycleValue.maxCycleValue;
        float lerpTime = LerpScreenCycleValue.lerpTime;


        if (minCycleValue == 0)
        {
            GameActionManager.instance.QueueAction(new HideAllPanel { hide = true }, true);
        }

        while (timeValue <= lerpTime)
        {
            float value = math.lerp(minCycleValue, maxCycleValue, timeValue / lerpTime);
            SetScreenCycleValue(value);
            yield return waitTime;
            timeValue += Time.fixedDeltaTime;
        }
        SetScreenCycleValue(maxCycleValue);
        if (LerpScreenCycleValue.setResult != null)
        {
            LerpScreenCycleValue.setResult(true);
        }

        if (maxCycleValue == 0)
        {
            GameActionManager.instance.QueueAction(new HideAllPanel { hide = false }, true);
        }
    }

    public void SetScreenCycleValue(float value)
    {
        screenMat.SetFloat("_CycleValue", value);
    }

  
    protected override void UpData()
    { 
        base.UpData();
    }
}