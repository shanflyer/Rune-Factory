using System;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera { get; private set; }
    public Camera uiCamera { get; private set; }
    public Camera worldUICamera { get; private set; }
    UniversalAdditionalCameraData universalAdditionalCameraData;
    [SerializeField]
    private PixelPerfectCamera pixelPerfectCamera, UIPixelPerfectCamera,WorldUIPixelPerfectCamera;

    private CinemachineMixingCamera mixingCamera;
    private CinemachineCamera fixedCamera;
    private CinemachineCamera[] followCameras;
    private CinemachineConfiner2D confiner2D;
    private CinemachineCameraOffset cameraOffset;
    private CinemachineCameraOffset[] CinemachineCameraOffsets;
    AudioListener cameraAudioListener;
    Volume volume;
    DepthOfField DepthOfField;

    //CinemachinePositionComposer[] cinemachineFramingTransposers;
    public override bool NeedUpdata => true;
    public override bool NeedLateUpdata => true;
    public bool fixedView { get; private set; }
    
    public void RefreshDepthOfField()
    {
        if (DepthOfField != null)
        {
            DepthOfField.ReMapValueY.value = math.lerp(0.2f, 2.0f, GameVolumeManager.instance.DepthFieldValue);
        }
    }
    public override void Init()
    {
        base.Init();
        hideLayer = LayerMask.NameToLayer("Hide");
        mainCamera = Camera.main;
        if (mainCamera.transform.childCount > 0)
        {
            worldUICamera = mainCamera.transform.GetChild(0).GetComponent<Camera>();
            WorldUIPixelPerfectCamera = worldUICamera.GetComponent<PixelPerfectCamera>();
        }
        volume= mainCamera.GetComponent<Volume>();
        volume.sharedProfile.TryGet<DepthOfField>(out DepthOfField);
        RefreshDepthOfField();

        cameraAudioListener =mainCamera.GetComponent<AudioListener>();
        uiCamera = mainCamera.transform.parent.GetChild(0).GetComponent<Camera>();
        universalAdditionalCameraData = uiCamera.GetComponent<UniversalAdditionalCameraData>();
        pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();
        UIPixelPerfectCamera= uiCamera.GetComponent<PixelPerfectCamera>();
        mixingCamera = mainCamera.transform.parent.GetComponentInChildren<CinemachineMixingCamera>();
        followCameras = new CinemachineCamera[3]
        {
            (CinemachineCamera)mixingCamera.ChildCameras[0],
            (CinemachineCamera)mixingCamera.ChildCameras[1],
            (CinemachineCamera)mixingCamera.ChildCameras[2]
        };
        fixedCamera = (CinemachineCamera)mixingCamera.ChildCameras[3];
        cameraOffset = fixedCamera.GetComponent<CinemachineCameraOffset>();
        confiner2D = mixingCamera.GetComponentInChildren<CinemachineConfiner2D>();
        CinemachineCameraOffsets = new CinemachineCameraOffset[3]
        {
            mixingCamera.ChildCameras[0].GetComponent<CinemachineCameraOffset>(),
            mixingCamera.ChildCameras[1].GetComponent<CinemachineCameraOffset>(),
            mixingCamera.ChildCameras[2].GetComponent<CinemachineCameraOffset>()
        };

        /* cinemachineFramingTransposers = new CinemachinePositionComposer[3]
         {
              //followCameras[0].GetCinemachineComponent(CinemachineCore.Stage.),
             //followCameras[1].GetCinemachineComponent<CinemachinePositionComposer>(),
            //  followCameras[2].GetCinemachineComponent<CinemachinePositionComposer>()
         };*/
        GameActionManager.instance.AddListener<SetFixedPlayerShaderPos>(SetFixedPlayerShaderPos);
        GameActionManager.instance.AddListener<SetFixedCamera>(SetFixedCamera);
        GameActionManager.instance.AddListener<SetCameraPixelValue>(SetCameraPixelValue);
        GameActionManager.instance.AddListener<SetCameraConfiner2D>(SetCameraConfiner2D);
         
    }
    void SetCameraConfiner2D(SetCameraConfiner2D SetCameraConfiner2D)
    {
        if (!fixedView)
        {
            confiner2D.enabled = SetCameraConfiner2D.enable;
            if (confiner2D.enabled)
            {
                confiner2D.InvalidateBoundingShapeCache();
                confiner2D.InvalidateLensCache();
            }
        } 
    }
    public void SetCameraListener(bool enable)
    {
        cameraAudioListener.enabled = enable;
    }
    /// <summary>
    /// 世界坐标转换为屏幕坐标
    /// </summary>
    /// <param name="worldPoint">屏幕坐标</param>
    /// <returns></returns>
    public static Vector2 WorldPointToScreenPoint(Vector3 worldPoint)
    {
        // Camera.main 世界摄像机
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPoint);
        return screenPoint;
    }

    /// <summary>
    /// 屏幕坐标转换为世界坐标
    /// </summary>
    /// <param name="screenPoint">屏幕坐标</param>
    /// <param name="planeZ">距离摄像机 Z 平面的距离</param>
    /// <returns></returns>
    public static Vector3 ScreenPointToWorldPoint(Vector2 screenPoint, float planeZ)
    {
        // Camera.main 世界摄像机
        Vector3 position = new Vector3(screenPoint.x, screenPoint.y, planeZ);
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(position);
        return worldPoint;
    }

    // RectTransformUtility.WorldToScreenPoint
    // RectTransformUtility.ScreenPointToWorldPointInRectangle
    // RectTransformUtility.ScreenPointToLocalPointInRectangle
    // 上面三个坐标转换的方法使用 Camera 的地方
    // 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 传递参数 canvas.worldCamera
    // 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 传递参数 null

    // UI 坐标转换为屏幕坐标
    public static Vector2 UIPointToScreenPoint(Vector3 worldPoint)
    {
        // RectTransform：target
        // worldPoint = target.position;
        Camera uiCamera = instance.uiCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldPoint);
        return screenPoint;
    }

    // 屏幕坐标转换为 UGUI 坐标
    public static Vector3 ScreenPointToUIPoint(RectTransform rt, Vector2 screenPoint)
    {
        Vector3 globalMousePos;
        //UI屏幕坐标转换为世界坐标
        Camera uiCamera = instance.uiCamera;

        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceCamera、RenderMode.WorldSpace 时 uiCamera 不能为空
        // 当 Canvas renderMode 为 RenderMode.ScreenSpaceOverlay 时 uiCamera 可以为空
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, screenPoint, uiCamera, out globalMousePos);
        return globalMousePos;
    }

    // 屏幕坐标转换为 UGUI RectTransform 的 anchoredPosition
    public static bool ScreenPointToUILocalPoint(RectTransform parentRT, Vector2 screenPoint, out Vector2 localPos)
    {
        Camera uiCamera = instance.uiCamera;

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, screenPoint, uiCamera, out localPos);
    }
    public void SetUICameraPostProcessing(bool ON)
    {
        universalAdditionalCameraData.renderPostProcessing = ON;
    }
    public void MoveFixedCamera(Vector2 movePos)
    {
        fixedCamera.transform.Translate(movePos);
    }

    public void SetConfiner2DCollider(Collider2D collider2D)
    {
        confiner2D.enabled = false;
        confiner2D.BoundingShape2D = collider2D;
        if (!UIManager.instance.filmUI||GameGuideManager.instance.endGuideFilmIndex>=GameDataManager.instance.GlobalData.endGuideIndex)
        {
            confiner2D.enabled = true;
            confiner2D.InvalidateBoundingShapeCache();
            confiner2D.InvalidateLensCache();
        } 
    }

    public void SetCameraOffset(Vector2 offset)
    {
        for (int i = 0; i < CinemachineCameraOffsets.Length; i++)
        {
            CinemachineCameraOffsets[i].Offset = offset;
        }
    }

    public void SetFollowTarget(Transform target)
    {
        for (int i = 0; i < followCameras.Length; i++)
        {
            followCameras[i].Follow = target;
            followCameras[i].Lens.OrthographicSize = pixelPerfectCamera.orthographicSize;
        }
        if(GameGuideManager.instance.endGuideFilmIndex >= GameDataManager.instance.GlobalData.endGuideIndex)
        {
            confiner2D.enabled = true;
            confiner2D.InvalidateBoundingShapeCache();
            confiner2D.InvalidateLensCache();
        }
       
    }

    private void SetCameraPixelValue(SetCameraPixelValue setCameraPixelValue)
    {
        WorldUIPixelPerfectCamera.assetsPPU= UIPixelPerfectCamera.assetsPPU = pixelPerfectCamera.assetsPPU = setCameraPixelValue.pixelValue;
        confiner2D.InvalidateLensCache();
    }

    private void SetFixedCamera(SetFixedCamera setFixedCamera)
    {
        WorldUIPixelPerfectCamera.assetsPPU = UIPixelPerfectCamera.assetsPPU = pixelPerfectCamera.assetsPPU = setFixedCamera.pixelValue == 0 ? GameCommon.PixelCameraDefaultValue : setFixedCamera.pixelValue;
        if (setFixedCamera.fixedCamera)
        {
            fixedView = true;
            mixingCamera.SetWeight(0, 0);
            mixingCamera.SetWeight(1, 0);
            mixingCamera.SetWeight(2, 0);
            mixingCamera.SetWeight(3, 1);
            if (setFixedCamera.fixedPos.x != float.MinValue)
            {
                Vector3 localPos = fixedCamera.transform.position;
                setFixedCamera.fixedPos.z = localPos.z;
                if (setFixedCamera.fixedPos.x != -1000)
                {
                    fixedCamera.transform.position = setFixedCamera.fixedPos;
                }
                else
                {
                    fixedCamera.transform.position = followCameras[0].transform.position;
                }
               
            }
            cameraOffset.Offset = setFixedCamera.offsetPos;
            confiner2D.enabled = false;
            //confiner2D.InvalidateBoundingShapeCache();
           // confiner2D.InvalidateLensCache();
        }
        else
        { 
            fixedView = false;
            int flowCameraIndex = (int)setFixedCamera.flowCameraType;
            followCameras[flowCameraIndex].Follow = null;
            if (flowCameraIndex == 0)
            {
                mixingCamera.SetWeight(0, 1);
                mixingCamera.SetWeight(1, 0);
                mixingCamera.SetWeight(2, 0);
                mixingCamera.SetWeight(3, 0);
                followCameras[flowCameraIndex].Follow = CharacterManager.instance.controllerTransform;
            }
            else
            {
                mixingCamera.SetWeight(0, 0);
                mixingCamera.SetWeight(1, 0);
                mixingCamera.SetWeight(2, 0);
                mixingCamera.SetWeight(3, 1);

                GameTimerController.instance.DelayAction(100, () =>
                {
                    followCameras[flowCameraIndex].Follow = CharacterManager.instance.controllerTransform;
                    mixingCamera.SetWeight((int)setFixedCamera.flowCameraType, 1);
                    mixingCamera.SetWeight(3, 0);
                });
            }
            if (!UIManager.instance.filmUI|| GameGuideManager.instance.endGuideFilmIndex >= GameDataManager.instance.GlobalData.endGuideIndex)
            {
                confiner2D.enabled = true;
                confiner2D.InvalidateBoundingShapeCache();
                confiner2D.InvalidateLensCache();
            } 
        }
    }
    void SetFixedPlayerShaderPos(SetFixedPlayerShaderPos setFixedPlayerShaderPos)
    {
        fixedPlayerShaderPos = setFixedPlayerShaderPos.fixedPos;
    }
    [SerializeField]
    bool fixedPlayerShaderPos = false;
   // private bool fixedView = false;
    public Vector3 oldCameraPos { get; private set; }

    protected override void UpData()
    {
        base.UpData();
        if (oldCameraPos != mainCamera.transform.position)
        {
            oldCameraPos = mainCamera.transform.position;
            EnvironmentManger.instance.SetCameraPos(oldCameraPos);
        }
    }

    public void AddTestRender(Renderer renderer)
    { 
        TestRenderers.TrySetValue(renderer,renderer.gameObject.layer);
    }
    public void RemoveTestRender(Renderer renderer)
    {
        if(TestRenderers.TryGetValue(renderer,out var layer))
        {
            renderer.gameObject.layer = layer;
        }
        TestRenderers.Remove(renderer);
    }
    MyDic<Renderer,int> TestRenderers = new MyDic<Renderer, int>();
    LayerMask hideLayer;
    protected override void LateUpData()
    {
        base.LateUpData();
        if (!fixedPlayerShaderPos)
        {
            if (CharacterManager.instance.ControllerRuntimeObj != null)
            {
                if (mixingCamera.Weight3 == 1&&(!CharacterManager.instance.ControllerRuntimeObj.gameObject.activeSelf))
                {
                    Shader.SetGlobalVector("_PlayerPos", fixedCamera.transform.position);
                }
                else if (CharacterManager.instance.ControllerRuntimeObj != null)
                {

                    Shader.SetGlobalVector("_PlayerPos", CharacterManager.instance.ControllerRuntimeObj.transform.position);
                }
            }
            else
            {
                Shader.SetGlobalVector("_PlayerPos", fixedCamera.transform.position);
            }
            
        }
        
         
        var planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        for(int i = 0; i < TestRenderers.length; i++)
        {
            var renderer = TestRenderers.GetKeyForIndex(i);
            bool enable= GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
            renderer.enabled = enable;
            
            if (enable)
            {
                renderer.gameObject.layer = TestRenderers[i];
            }
            else
            {
                renderer.gameObject.layer = hideLayer;
            }
          //  renderer.gameObject.layer = 1;
          // renderer.enabled = GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        } 
    }

}