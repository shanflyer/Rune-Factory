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
    public override void Init()
    {
        base.Init();
        mainCamera = Camera.main;
        uiCamera = mainCamera.transform.GetChild(0).GetComponent<Camera>();

        pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();
        mixingCamera = mainCamera.transform.parent.GetComponentInChildren<CinemachineMixingCamera>();
        followCamera = (CinemachineVirtualCamera)mixingCamera.ChildCameras[0];
        fixedCamera = (CinemachineVirtualCamera)mixingCamera.ChildCameras[1];
    }
    public void SetFollowTarget(Transform target)
    {
        followCamera.Follow = target;
        followCamera.m_Lens.OrthographicSize = pixelPerfectCamera.orthographicSize;
    }
}