using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameVolumeManager : Singleton<GameVolumeManager>
{ 
    Material screenMat; 
    public override void Init()
    {
        base.Init();
        UniversalRenderPipelineAsset universalRenderPipelineAsset = UniversalRenderPipeline.asset;
        var Renderer2DData= universalRenderPipelineAsset.RendererDataList[0];
        var rendererFeature= Renderer2DData.rendererFeatures.Find(r => r.name == "ScreenCycle");
        screenMat = ((FullScreenPassRendererFeature)rendererFeature).passMaterial;
        GameActionManager.instance.AddListener<LerpScreenCycleValue>(LerpScreenCycleValue);
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
        GameObjectCurveController.instance.UpDataComponent.StartCoroutine(enumerator);
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

            while (timeValue<= lerpTime)
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
}