using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class GameObjectCurveController 
{
    public static GameObjectCurveController instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObjectCurveController();
            }
            return _instance;
        }
    }
    public static GameObjectCurveController _instance;

    public delegate Vector2 GetCurvePos(float timeValue);

    public delegate void CurveAction(Vector2 pos);
    public delegate void CurveEndAction();
    private Dictionary<int, IEnumerator> objectMoveIEnumerator = new Dictionary<int, IEnumerator>();

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

        GameController.instance.StartCoroutine(enumerator);
        return enumerator;
    }

    public IEnumerator Line(float speed, Vector2 startPos,Vector2 targetPos, CurveAction curveAction, CurveEndAction curveEndAction)
    { 
        IEnumerator enumerator = CurveAddTime(speed, curveAction, (float timeValue) => {

            Vector2 pos = startPos + (targetPos - startPos) * timeValue;
            return pos;
        }, curveEndAction);

        GameController.instance.StartCoroutine(enumerator);

        return enumerator;
    }
    public void StopObjectMove(int instanceId)
    {
        if (objectMoveIEnumerator.TryGetValue(instanceId, out IEnumerator enumerator))
        {
            GameController.instance.StopCoroutine(enumerator);
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
            GameController.instance.StartCoroutine(enumerator);
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
                if (WorldMapContorller.instance.InitSmoothMove(ref direction, nowPos, mapId))
                {
                    Vector2 targetPos = nowPos + direction * CharacterManager.moveSpeed * Time.fixedDeltaTime;
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
                Vector2 targetPos = nowPos + direction * CharacterManager.moveSpeed * Time.fixedDeltaTime;
                int2 targetCoordinate = GameCommon.GetMapCoordinateInt(targetPos);
                SetMoveTarge(targetCoordinate, targetPos);
            }

            yield return new WaitForFixedUpdate();
        }
    }

}
