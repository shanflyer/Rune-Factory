using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

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

public class GameObjectCurveController : Singleton<GameObjectCurveController>
{
    public delegate Vector2 GetCurvePos(float timeValue);

    public delegate void SetCurvePosCurveMoveData(float timeValue, CurveMoveData curveMoveData);

    public delegate void CurveAction(Vector2 pos);

    public delegate void CurveEndAction();

    private Dictionary<int, IEnumerator> objectMoveIEnumerator = new Dictionary<int, IEnumerator>();
    private Dictionary<int, IEnumerator> runIEnumerator = new Dictionary<int, IEnumerator>();
    private Dictionary<int, IEnumerator> pauseEnumerator = new Dictionary<int, IEnumerator>();
    private MyInstance myInstance;

    public MonoBehaviour UpDataComponent;

    public void RemoveLineMove(int instanceId)
    {
        if (runIEnumerator.TryGetValue(instanceId, out var enumerator))
        {
            UpDataComponent.StopCoroutine(enumerator);
            runIEnumerator.Remove(instanceId);
        }
        if (pauseEnumerator.TryGetValue(instanceId, out enumerator))
        {
            pauseEnumerator.Remove(instanceId);
        }
    }

    public bool StopLineMove(int instanceId)
    {
        if (!pauseEnumerator.ContainsKey(instanceId) && runIEnumerator.TryGetValue(instanceId, out var enumerator))
        {
            UpDataComponent.StopCoroutine(enumerator);
            runIEnumerator.Remove(instanceId);
            pauseEnumerator.Add(instanceId, enumerator);
            return true;
        }
        return false;
    }

    public bool StartLineMove(int instanceId)
    {
        if (!runIEnumerator.ContainsKey(instanceId) && pauseEnumerator.TryGetValue(instanceId, out var enumerator))
        {
            UpDataComponent.StartCoroutine(enumerator);
            pauseEnumerator.Remove(instanceId);
            runIEnumerator.Add(instanceId, enumerator);

            return true;
        }
        return false;
    }

    public void SetUpDataComponent(MonoBehaviour UpDataComponent)
    {
        this.UpDataComponent = UpDataComponent;
    }

    protected override void Clear()
    {
        base.Clear();
        myInstance.Clear();
    }

    public override void Init()
    {
        base.Init();
        myInstance = new MyInstance();
    }

    private void SetCurvePosCurveMoveDataAction(float timeValue, CurveMoveData curveMoveData)
    {
        float oneMinusTime = 1 - timeValue;

        Vector2 pos = oneMinusTime * oneMinusTime * curveMoveData.startPos + 2 * timeValue * oneMinusTime * curveMoveData.middlePos
        + timeValue * timeValue * curveMoveData.targetPos;
        curveMoveData.transform.position = pos;
    }

    private void SetLinePosCurveMoveDataAction(float timeValue, CurveMoveData curveMoveData)
    {
        Vector2 pos = curveMoveData.startPos + (curveMoveData.targetPos - curveMoveData.startPos) * timeValue;
        curveMoveData.transform.position = pos;
    }

    private IEnumerator CurveAddTimeList(List<CurveMoveData> curveMoveDatas, SetCurvePosCurveMoveData SetCurvePos,
        CurveEndAction curveEndAction)
    {
        float timeValue = 0;
        float deltaValue = Time.fixedDeltaTime;
        int endCount = 0;
        var WaitForFixedUpdate = new WaitForFixedUpdate();
        while (endCount < curveMoveDatas.Count)
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
                        curveMoveData.transform.localScale = Vector3.one;
                        float lerpValue = (timeValue - curveMoveData.waitTime) / curveMoveData.moveTime;
                        SetCurvePos(lerpValue, curveMoveData);
                    }
                }
                else
                {
                    endCount++;
                }
            }
            yield return WaitForFixedUpdate;
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
        var WaitForFixedUpdate=new WaitForFixedUpdate();
        while (timeValue < 1)
        {
            timeValue += deltaValue;
            yield return WaitForFixedUpdate;
            curveAction(getCurvePos(timeValue));
        }
        if (curveEndAction != null)
        {
            curveEndAction();
        }
    }

    public IEnumerator Curve(float speed, Vector2 startPos, Vector2 targetPos, Vector2 middlePos, CurveAction curveAction, CurveEndAction curveEndAction)
    {
        IEnumerator enumerator = CurveAddTime(speed, curveAction, (float timeValue) =>
        {
            float oneMinusTime = 1 - timeValue;

            Vector2 pos = oneMinusTime * oneMinusTime * startPos + 2 * timeValue * oneMinusTime * middlePos
            + timeValue * timeValue * targetPos;
            return pos;
        }, curveEndAction);

        UpDataComponent.StartCoroutine(enumerator);
        return enumerator;
    }

    public int Line(float speed, Vector2 startPos, Vector2 targetPos, CurveAction curveAction, CurveEndAction curveEndAction)
    {
        int instanceId = myInstance.CreatInstanceId();
        IEnumerator enumerator = CurveAddTime(speed, curveAction, (float timeValue) =>
        {
            Vector2 pos = startPos + (targetPos - startPos) * timeValue;
            return pos;
        }, EnnAction);

        void EnnAction()
        {
            if (curveEndAction != null)
            {
                curveEndAction();
            }
            runIEnumerator.Remove(instanceId);
            pauseEnumerator.Remove(instanceId);
            myInstance.RemoveInstance(instanceId);
        }

        UpDataComponent.StartCoroutine(enumerator);
        runIEnumerator.Add(instanceId, enumerator);

        return instanceId;
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
    public void ObjectMove(Character character, GetMoveVector GetObjectPos, SetMoveTarge SetMoveTarge, int instanceId)
    {
        if (!objectMoveIEnumerator.TryGetValue(instanceId, out IEnumerator enumerator))
        {
            enumerator = ObjectMoving(character,GetObjectPos, SetMoveTarge, instanceId);
            UpDataComponent.StartCoroutine(enumerator);
            objectMoveIEnumerator.Add(instanceId, enumerator);
        }
    }
    public void ObjectMove(GetMoveVector GetObjectPos, GetMoveVector GetMoveDirction, SetMoveTarge SetMoveTarge,
        int mapId, int instanceId, bool checkWalk,float speed=1)
    {
        if (!objectMoveIEnumerator.TryGetValue(instanceId, out IEnumerator enumerator))
        {
            enumerator = ObjectFreedomMoving(GetObjectPos, GetMoveDirction, SetMoveTarge, mapId, instanceId, checkWalk, speed);
            UpDataComponent.StartCoroutine(enumerator);
            objectMoveIEnumerator.Add(instanceId, enumerator);
        }
    }
    
    private IEnumerator ObjectMoving(Character character, GetMoveVector GetObjectPos, SetMoveTarge SetMoveTarge, int instanceId)
    { 
        bool _continue = true;
        while (!character.moveDirection.Equals(float2.zero))
        {
            Vector2 direction = character.moveDirection;
            Vector2 nowPos = GetObjectPos();
            float speed = character.propertySpeed;

            Vector2 targetPos = nowPos + direction * speed * GameCommon.freedomMoveValue * Time.deltaTime; 
            int2 targetCoordinate = GameCommon.GetMapCoordinateInt(targetPos);
            int2 trueTargetCoordinate = MapCellController.instance.GetTrueFreedomTarget(character.coordinate, targetCoordinate, character.mapInstance);
            if (direction != Vector2.zero && trueTargetCoordinate.Equals(character.coordinate))
            {

            }

            if (!trueTargetCoordinate.Equals(targetCoordinate))
            {
                if (!trueTargetCoordinate.Equals(character.coordinate))
                {
                    targetPos = GameCommon.GetMapPos(trueTargetCoordinate); 
                }
                else
                {
                    targetPos = nowPos;
                    // _continue = false;
                    //StopObjectMove(instanceId);
                }
            }
            SetMoveTarge(trueTargetCoordinate, targetPos);

            yield return 0;
        }
        StopObjectMove(instanceId);
    }
     

    private IEnumerator ObjectFreedomMoving(GetMoveVector GetObjectPos, GetMoveVector GetMoveDirction, SetMoveTarge SetMoveTarge,
        int mapId, int instanceId, bool checkWalk,float speed= 1)
    {
        bool _continue = true;
        while (_continue)
        {
            Vector2 direction = GetMoveDirction();
            Vector2 nowPos = GetObjectPos();

            if (checkWalk)
            {
                int2 target = int2.zero;
                Vector2 targetPos = nowPos;

                float distance = CharacterManager.updataMoveSpeed * Time.deltaTime* speed;

                if (WorldMapManager.instance.InitSmoothMove(ref direction, nowPos, mapId, distance, ref target, ref targetPos))
                {
                    SetMoveTarge(target, targetPos);
                }
                else
                {
                    _continue = false;
                    StopObjectMove(instanceId);
                }
            }
            else
            {
                Vector2 targetPos = nowPos + direction * CharacterManager.updataMoveSpeed * Time.deltaTime * speed;
                int2 targetCoordinate = GameCommon.GetMapCoordinateInt(targetPos);
                SetMoveTarge(targetCoordinate, targetPos);
            }

            yield return 0;
        }
    }
}