using System.Collections;
using UnityEngine;

public class CameraController:Singleton<CameraController>
{
    public Camera mainCamera;
    public Camera uiCamera;

    public override void Init()
    {
        base.Init();
        mainCamera = Camera.main;
        uiCamera = mainCamera.transform.GetChild(0).GetComponent<Camera>();
    }
}