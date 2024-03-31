using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera;
    public Camera uiCamera;

    [SerializeField]
    private PixelPerfectCamera pixelPerfectCamera;

    private CinemachineMixingCamera mixingCamera;
    private CinemachineVirtualCamera fixedCamera;
    private CinemachineVirtualCamera[] followCameras;
    private CinemachineConfiner2D confiner2D;

    private CinemachineCameraOffset[] CinemachineCameraOffsets;
    CinemachineFramingTransposer[] cinemachineFramingTransposers;
    public override bool NeedUpdata => true;

    public override void Init()
    {
        base.Init();
        mainCamera = Camera.main;
        uiCamera = mainCamera.transform.GetChild(0).GetComponent<Camera>();

        pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();
        mixingCamera = mainCamera.transform.parent.GetComponentInChildren<CinemachineMixingCamera>();
        followCameras = new CinemachineVirtualCamera[3]
        {
            (CinemachineVirtualCamera)mixingCamera.ChildCameras[0],
            (CinemachineVirtualCamera)mixingCamera.ChildCameras[1],
            (CinemachineVirtualCamera)mixingCamera.ChildCameras[2]
        };
        fixedCamera = (CinemachineVirtualCamera)mixingCamera.ChildCameras[3];
        confiner2D = mixingCamera.GetComponentInChildren<CinemachineConfiner2D>();
        CinemachineCameraOffsets = new CinemachineCameraOffset[3]
        {
            mixingCamera.ChildCameras[0].GetComponent<CinemachineCameraOffset>(),
            mixingCamera.ChildCameras[1].GetComponent<CinemachineCameraOffset>(),
            mixingCamera.ChildCameras[2].GetComponent<CinemachineCameraOffset>()
        };

        cinemachineFramingTransposers = new CinemachineFramingTransposer[3]
        {
             followCameras[0].GetCinemachineComponent<CinemachineFramingTransposer>(),
             followCameras[1].GetCinemachineComponent<CinemachineFramingTransposer>(),
             followCameras[2].GetCinemachineComponent<CinemachineFramingTransposer>()
        };

        GameActionManager.instance.AddListener<SetFixedCamera>(SetFixedCamera);
    }

    public void SetConfiner2DCollider(PolygonCollider2D polygonCollider2D)
    {
        confiner2D.enabled = false;
        confiner2D.m_BoundingShape2D = polygonCollider2D;
        confiner2D.enabled = true;
        confiner2D.InvalidateCache();
        GameTimerController.instance.DeleyActionMain(100, () =>
        {
            confiner2D.enabled = true;
            confiner2D.InvalidateCache();
            confiner2D.enabled = false;
            confiner2D.enabled = true;
        });
        GameTimerController.instance.DeleyActionMain(200, () =>
        {
            confiner2D.InvalidateCache();
        });
    }

    public void SetCameraOffset(Vector2 offset)
    {
        for (int i = 0; i < CinemachineCameraOffsets.Length; i++)
        {
            CinemachineCameraOffsets[i].m_Offset = offset;
        }
    }

    public void SetFollowTarget(Transform target)
    {
        for (int i = 0; i < followCameras.Length; i++)
        {
            followCameras[i].Follow = target;
            followCameras[i].m_Lens.OrthographicSize = pixelPerfectCamera.orthographicSize;
        }
    }

    private void SetFixedCamera(SetFixedCamera setFixedCamera)
    {
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
                fixedCamera.transform.position = setFixedCamera.fixedPos;
            }
            confiner2D.enabled = false;
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
                
                GameTimerController.instance.DeleyActionMain(100, () =>
                {
                    
                    followCameras[flowCameraIndex].Follow = CharacterManager.instance.controllerTransform;
                    mixingCamera.SetWeight((int)setFixedCamera.flowCameraType, 1);
                    mixingCamera.SetWeight(3, 0);
                });
            }
            confiner2D.enabled = true;
            confiner2D.InvalidateCache();
        }
    }

    private bool fixedView = false;
    private Vector3 oldCameraPos;

    protected override void UpData()
    {
        base.UpData();
        if (oldCameraPos != mainCamera.transform.position)
        {
            oldCameraPos = mainCamera.transform.position;
            EnvironmentManger.instance.SetCameraPos(oldCameraPos);
        }
    }
}