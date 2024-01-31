using System.Collections;
using UnityEngine;
using Cinemachine;
using UnityEngine.Experimental.Rendering.Universal;

public class CameraManager : Singleton<CameraManager>
{
    public Camera mainCamera;
    public Camera uiCamera;
    [SerializeField]
    private PixelPerfectCamera pixelPerfectCamera;

    private CinemachineMixingCamera mixingCamera;
    private CinemachineVirtualCamera followCamera, fixedCamera;
    private CinemachineConfiner2D confiner2D;
    public override void Init()
    {
        base.Init();
        mainCamera = Camera.main;
        uiCamera = mainCamera.transform.GetChild(0).GetComponent<Camera>();

        pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();
        mixingCamera = mainCamera.transform.parent.GetComponentInChildren<CinemachineMixingCamera>();
        followCamera = (CinemachineVirtualCamera)mixingCamera.ChildCameras[0];
        fixedCamera = (CinemachineVirtualCamera)mixingCamera.ChildCameras[1];
        confiner2D = mixingCamera.GetComponentInChildren<CinemachineConfiner2D>();

        GameActionManager.instance.AddListener<SetFixedCamera>(SetFixedCamera);
    }
    public void SetConfiner2DCollider(PolygonCollider2D polygonCollider2D)
    {
        confiner2D.enabled = false;
        confiner2D.m_BoundingShape2D= polygonCollider2D;
        confiner2D.enabled = true;
        GameTimerController.instance.DeleyActionMain(100, () =>
        {
            confiner2D.InvalidateCache();
        });
        
    }
    
    public void SetFollowTarget(Transform target)
    {
        followCamera.Follow = target;
        followCamera.m_Lens.OrthographicSize = pixelPerfectCamera.orthographicSize;
    }
    void SetFixedCamera(SetFixedCamera setFixedCamera)
    {
        if (setFixedCamera.fixedCamera)
        {
            mixingCamera.SetWeight(0, 0);
            mixingCamera.SetWeight(1, 1);
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
            mixingCamera.SetWeight(0, 1);
            mixingCamera.SetWeight(1, 0);
            followCamera.Follow = CharacterManager.instance.controllerTransform;
            confiner2D.enabled = true;
        }
    }
}