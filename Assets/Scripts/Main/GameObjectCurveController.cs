using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

public struct CurveMoveData
{
    public float waitTime;
    public float moveTime;
    public Transform transform;
    public Vector2 startPos;
    public Vector2 targetPos;
    public Vector2 middlePos;
    public GameObjectCurveController.CurveEndAction CurveEndAction;

}
public class GameObjectCurveController:Singleton<GameObjectCurveController>
{ 
    public delegate Vector2 GetCurvePos(float timeValue);
    public delegate void SetCurvePosCurveMoveData(float timeValue, CurveMoveData curveMoveData);

    public delegate void CurveAction(Vector2 pos);
    public delegate void CurveEndAction();
    private Dictionary<int, IEnumerator> objectMoveIEnumerator = new Dictionary<int, IEnumerator>();

    private MonoBehaviour UpDataComponent;

    public void StopMove(IEnumerator enumerator)
    {
        UpDataComponent.StopCoroutine(enumerator);
    }
    public void StartMove(IEnumerator enumerator)
    { 
        UpDataComponent.StartCoroutine(enumerator);
    }
    public void SetUpDataComponent(MonoBehaviour UpDataComponent)
    {
        this.UpDataComponent=UpDataComponent; 
    }
    protected override void Clear()
    {
        base.Clear();
    }
    public override void Init()
    {
        base.Init();
    }


    void SetCurvePosCurveMoveDataAction(float timeValue, CurveMoveData curveMoveData)
    {
        float oneMinusTime = 1 - timeValue;

        Vector2 pos = oneMinusTime * oneMinusTime * curveMoveData.startPos + 2 * timeValue * oneMinusTime * curveMoveData.middlePos
        + timeValue * timeValue * curveMoveData.targetPos;
        curveMoveData.transform.position=pos;
    }
    void SetLinePosCurveMoveDataAction(float timeValue, CurveMoveData curveMoveData)
    { 
        Vector2 pos = curveMoveData.startPos +(curveMoveData.targetPos-curveMoveData.startPos)*timeValue;
        curveMoveData.transform.position = pos;
    }
    private IEnumerator CurveAddTimeList(List<CurveMoveData> curveMoveDatas, SetCurvePosCurveMoveData SetCurvePos,
        CurveEndAction curveEndAction)
    {
        float timeValue = 0;
        float deltaValue = Time.fixedDeltaTime;
        int endCount = 0;
        while (endCount>=curveMoveDatas.Count)
        {
            timeValue += deltaValue;
            endCount = 0;
            for (int i = 0; i < curveMoveDatas.Count; i++)
            {
                var curveMoveData = curveMoveDatas[i];
                if (curveMoveData.transform != null)
                {
                    if (timeValue >= curveMoveData.waitTime + curveMoveData.moveTime)
                    {
                        if (curveMoveData.CurveEndAction != null)
                        {
                            curveMoveData.CurveEndAction.Invoke();
                        }
                        curveMoveData.transform = null;
                        endCount++;
                    }
                    else if (timeValue >= curveMoveData.waitTime)
                    {
                        float lerpValue = (timeValue - curveMoveData.waitTime) / curveMoveData.moveTime;
                        SetCurvePos(lerpValue, curveMoveData);
                    }
                }
                else
                {
                    endCount++;
                }
            }   
            yield return new WaitForFixedUpdate(); 
        }
        if (curveEndAction != null)
        {
            curveEndAction();
        } 
    }
    public IEnumerator CurveList(List<CurveMoveData> curveMoveDatas, CurveEndAction curveEndAction)
    {
        IEnumerator enumerator = CurveAddTimeList(curveMoveDatas, SetCurvePosCurveMoveDataAction, curveEndAction);

        UpDataComponent.StartCoroutine(enumerator);
        return enumerator;
    }
    public IEnumerator LineList(List<CurveMoveData> curveMoveDatas, CurveEndAction curveEndAction)
    {
        IEnumerator enumerator = CurveAddTimeList(curveMoveDatas, SetLinePosCurveMoveDataAction, curveEndAction);

        UpDataComponent.StartCoroutine(enumerator);
        return enumerator;
    }



    private IEnumerator CurveAddTime(float speed, CurveAction curveAction, GetCurvePos getCurvePos,
         CurveEndAction curveEndAction)
    {
        float timeValue = 0;
        float deltaValue = Time.fixedDeltaTime * speed;
        while (timeValue < 1)
        {
            timeValue += deltaValue;
            yield return new WaitForFixedUpdate();
            curveAction(getCurvePos(timeValue));
        }
        if (curveEndAction != null) ;
        {
            curveEndAction();
        }

    }
    public IEnumerator Curve(float speed,Vector2 startPos,Vector2 targetPos,Vector2 middlePos, CurveAction curveAction, CurveEndAction curveEndAction)
    {
        IEnumerator enumerator = CurveAddTime(speed, curveAction, (float timeValue) => {

            float oneMinusTime = 1- timeValue;

            Vector2 pos = oneMinusTime* oneMinusTime* startPos+2*timeValue*oneMinusTime* middlePos
            + timeValue*timeValue*targetPos;
            return pos;
        }, curveEndAction);

        UpDataComponent.StartCoroutine(enumerator);
        return enumerator;
    }

    public IEnumerator Line(float speed, Vector2 startPos,Vector2 targetPos, CurveAction curveAction, CurveEndAction curveEndAction)
    { 
        IEnumerator enumerator = CurveAddTime(speed, curveAction, (float timeValue) => {

            Vector2 pos = startPos + (targetPos - startPos) * timeValue;
            return pos;
        }, curveEndAction);

        UpDataComponent.StartCoroutine(enumerator);

        return enumerator;
    }
    public void StopObjectMove(int instanceId)
    {
        if (objectMoveIEnumerator.TryGetValue(instanceId, out IEnumerator enumerator))
        {
            UpDataComponent.StopCoroutine(enumerator);
            objectMoveIEnumerator.Remove(instanceId);
            enumerator = null;
           
        }
    }
    
    public void ObjectMove(GetMoveVector GetObjectPos,GetMoveVector GetMoveDirction,SetMoveTarge SetMoveTarge,
        int mapId, int instanceId, bool checkWalk)
    {
        if(!objectMoveIEnumerator.TryGetValue(instanceId, out IEnumerator enumerator))
        {
            enumerator = ObjectFreedomMoving(GetObjectPos,GetMoveDirction,SetMoveTarge,mapId,instanceId,checkWalk);
            UpDataComponent.StartCoroutine(enumerator);
            objectMoveIEnumerator.Add(instanceId, enumerator);
        }
    }

    private IEnumerator ObjectFreedomMoving(GetMoveVector GetObjectPos, GetMoveVector GetMoveDirction, SetMoveTarge SetMoveTarge,
        int mapId, int instanceId, bool checkWalk)
    {
        bool _continue = true;
        while (_continue)
        {
            Vector2 direction = GetMoveDirction();
            Vector2 nowPos = GetObjectPos();

            if (checkWalk)
            {
                if (WorldMapManager.instance.InitSmoothMove(ref direction, nowPos, mapId))
                {
                    Vector2 targetPos = nowPos + direction * CharacterManager.updataMoveSpeed * Time.fixedDeltaTime;
                    int2 targetCoordinate = GameCommon.GetMapCoordinateInt(targetPos);
                    SetMoveTarge(targetCoordinate, targetPos);
                }
                else
                {
                    _continue = false;
                    StopObjectMove(instanceId);
                }

            }
            else
            {
                Vector2 targetPos = nowPos + direction * CharacterManager.updataMoveSpeed * Time.fixedDeltaTime;
                int2 targetCoordinate = GameCommon.GetMapCoordinateInt(targetPos);
                SetMoveTarge(targetCoordinate, targetPos);
            }

            yield return new WaitForFixedUpdate();
        }
    }

}
