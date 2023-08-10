using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CamreaMoveTest : MonoBehaviour, IDragHandler
{
    public Transform MainCamera;
    public float coefficient;
    public float cameraMoveSpeed;
    //private float moveSpeed;
    private GameComponent GameData;
    private GameManager gameraManager;
    private MapEditAction mapEdit;
    private Boundary nowBoundary;
    public bool IsCameraMoveEnd;
    public bool IsCameraMoveStart;
    private Vector3 cameraToPos;
    public float yaoSpeed;

    public void SetBoundary()
    {
        nowBoundary = gameraManager.CreatBoundary();
        LimitCameraMove();
    }
    void LimitCameraMove()
    {
        float x = Mathf.Clamp(MainCamera.position.x, nowBoundary.MinX, nowBoundary.MaxX);
        float y = Mathf.Clamp(MainCamera.position.y, nowBoundary.MinY, nowBoundary.MaxY);
        MainCamera.position = new Vector3(x, y, MainCamera.position.z);
    }
    public void CameraPoscorrect(Vector3 pos, float percentage)
    {
        IsCameraMoveEnd = false;
      
        IsCameraMoveStart = false;
        MoveCamera(pos, percentage);
        
    }
   
    public void MoveCamera(Vector3 pos, float percentage,float speed)
    {
        float dh = 0, dw = 0;
        float sH = Screen.height;
        float sW = Screen.width;
        Vector2 objSp = Camera.main.WorldToScreenPoint(pos);
        if (objSp.y < sH * percentage * 0.5f)
        {
            dh = objSp.y - sH * percentage * 0.5f;
        }
        if (objSp.y > sH * (1 - percentage * 0.5f))
        {
            dh = objSp.y - sH * (1 - percentage * 0.5f);
        }
        if (objSp.x < sW * percentage * 0.5f)
        {
            dw = objSp.x - sW * percentage * 0.5f;
        }
        if (objSp.x > sW * (1 - percentage * 0.5f))
        {
            dw = objSp.x - sW * (1 - percentage * 0.5f);
        }
        Vector3 cameraSc = Camera.main.WorldToScreenPoint(Camera.main.transform.position);
        cameraSc = cameraSc + new Vector3(dw, dh, 0);
        cameraSc = Camera.main.ScreenToWorldPoint(cameraSc);
        cameraSc.x = Mathf.Min(cameraSc.x, nowBoundary.MaxX);
        cameraSc.x = Mathf.Max(cameraSc.x, nowBoundary.MinX);
        cameraSc.y = Mathf.Min(cameraSc.y, nowBoundary.MaxY);
        cameraSc.y = Mathf.Max(cameraSc.y, nowBoundary.MinY);
        cameraToPos = cameraSc;
        //moveSpeed = speed;
        StartCoroutine("CameraMove");
    }
    
    public void MoveCamera(Vector3 pos, float percentage)
    {
        float dh = 0, dw = 0;
        float sH = Screen.height;
        float sW = Screen.width;
        Vector2 objSp = Camera.main.WorldToScreenPoint(pos);
        if (objSp.y < sH * percentage * 0.5f)
        {
            dh = objSp.y - sH * percentage * 0.5f;
        }
        if (objSp.y > sH * (1 - percentage * 0.5f))
        {
            dh = objSp.y - sH * (1 - percentage * 0.5f);
        }
        if (objSp.x < sW * percentage * 0.5f)
        {
            dw = objSp.x - sW * percentage * 0.5f;
        }
        if (objSp.x > sW * (1 - percentage * 0.5f))
        {
            dw = objSp.x - sW * (1 - percentage * 0.5f);
        }
        Vector3 cameraSc = Camera.main.WorldToScreenPoint(Camera.main.transform.position);
        cameraSc = cameraSc + new Vector3(dw, dh, 0);
        cameraSc = Camera.main.ScreenToWorldPoint(cameraSc);
        cameraSc.x = Mathf.Min(cameraSc.x, nowBoundary.MaxX);
        cameraSc.x = Mathf.Max(cameraSc.x, nowBoundary.MinX);
        cameraSc.y = Mathf.Min(cameraSc.y, nowBoundary.MaxY);
        cameraSc.y = Mathf.Max(cameraSc.y, nowBoundary.MinY);
        cameraToPos=cameraSc;
        //moveSpeed = cameraMoveSpeed;
        StartCoroutine("CameraMove");
    }

    public void MoveX(Vector3 pos)
    {
        cameraToPos = pos;
        StartCoroutine("CameraMove");
    }
    public void MoveCamera(Vector3 pos)
    {
        MainCamera.Translate(pos* yaoSpeed);
       
        LimitCameraMove();
    }
    IEnumerator CameraMove()
    {
        while (true)
        {
            Vector3 distance = cameraToPos - Camera.main.transform.position;
            if (distance.magnitude <= 0.01f)
            {
                IsCameraMoveEnd = true;
                Camera.main.transform.position = cameraToPos;
                IsCameraMoveStart = true;
                

                //gameraManager.AttackContinue();
                StopCoroutine("CameraMove");

                break;
            }
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, cameraToPos, cameraMoveSpeed * Time.deltaTime);

            yield return new WaitForFixedUpdate();
        }
        
    }
    void Awake()
    {
        
        
    }

    public void InitData()
    {
        GameData=GameComponentData.gameData;
        mapEdit = GameData.mapEditAction;
        gameraManager = GameData.gameManager;
        IsCameraMoveEnd = true;
        IsCameraMoveStart = true;
        SetBoundary();
    }
    public void OnDrag(PointerEventData data)
    {
        /*
        Vector2 deltaPos = data.delta;
        if (mapEdit.isCamreaMove)
        {
            MainCamera.Translate(deltaPos * coefficient);
            LimitCameraMove();
        }
        */
    }

}  
