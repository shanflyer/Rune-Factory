using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CurveMoveData
{
    public float waitTime;
    public float moveTime;
    public Transform transform;
    public Vector3 startPos;
    public Vector3 targetPos;
    public Vector3 middlePos;
    public GameObjectCurveController.CurveEndAction CurveEndAction;
}

public class GameObjectCurveController : Singleton<GameObjectCurveController>
{
    public delegate void CurveAction(Vector3 pos);

    public delegate void CurveEndAction();

    public delegate bool FrameTaskAction(float deltaTime);

    public readonly struct MoveHandle
    {
        public readonly int id;
        public readonly int version;

        public MoveHandle(int id, int version)
        {
            this.id = id;
            this.version = version;
        }

        public bool IsValid => id > 0;
    }

    public readonly struct MoveGroupHandle
    {
        public readonly int id;
        public readonly int version;

        public MoveGroupHandle(int id, int version)
        {
            this.id = id;
            this.version = version;
        }

        public bool IsValid => id > 0;
    }

    public readonly struct FrameTaskHandle
    {
        public readonly int id;
        public readonly int version;

        public FrameTaskHandle(int id, int version)
        {
            this.id = id;
            this.version = version;
        }

        public bool IsValid => id > 0;
    }

    private enum TweenType
    {
        Line,
        Curve
    }

    private enum ObjectMoveType
    {
        Character,
        Freedom
    }

    private sealed class MoveGroup
    {
        public int id;
        public int version;
        public int remainingCount;
        public int listIndex;
        public bool paused;
        public CurveEndAction endAction;
    }

    private sealed class TweenTask
    {
        public int id;
        public int version;
        public int listIndex;
        public float elapsedTime;
        public float waitTime;
        public float moveTime;
        public bool paused;
        public TweenType tweenType;
        public Vector3 startPos;
        public Vector3 targetPos;
        public Vector3 middlePos;
        public Transform transform;
        public CurveAction curveAction;
        public CurveEndAction endAction;
        public MoveGroup group;
        public bool scaleOnStart;
        public bool scaleApplied;
    }

    private sealed class ObjectMoveTask
    {
        public int instanceId;
        public int listIndex;
        public ObjectMoveType moveType;
        public Character character;
        public GetVector2 getObjectPos;
        public GetVector2 getMoveDirection;
        public SetMoveTarge setMoveTarget;
        public int mapId;
        public bool checkWalk;
        public float speed;
    }

    private sealed class FrameTask
    {
        public int id;
        public int version;
        public int listIndex;
        public bool paused;
        public FrameTaskAction updateAction;
        public Action endAction;
    }

    private readonly List<TweenTask> tweenTasks = new List<TweenTask>();
    private readonly Dictionary<int, TweenTask> tweenTaskDic = new Dictionary<int, TweenTask>();
    private readonly List<MoveGroup> moveGroups = new List<MoveGroup>();
    private readonly Dictionary<int, MoveGroup> moveGroupDic = new Dictionary<int, MoveGroup>();
    private readonly List<ObjectMoveTask> objectMoveTasks = new List<ObjectMoveTask>();
    private readonly Dictionary<int, ObjectMoveTask> objectMoveTaskDic = new Dictionary<int, ObjectMoveTask>();
    private readonly List<FrameTask> frameTasks = new List<FrameTask>();
    private readonly Dictionary<int, FrameTask> frameTaskDic = new Dictionary<int, FrameTask>();
    private int nextMoveId;
    private int nextMoveVersion;
    private int nextMoveGroupId;
    private int nextMoveGroupVersion;
    private int nextFrameTaskId;
    private int nextFrameTaskVersion;

    public override bool NeedUpdate => true;

    public FrameTaskHandle StartFrameTask(FrameTaskAction updateAction, Action endAction = null)
    {
        if (updateAction == null)
        {
            return default;
        }

        var handle = new FrameTaskHandle(++nextFrameTaskId, ++nextFrameTaskVersion);
        var task = new FrameTask
        {
            id = handle.id,
            version = handle.version,
            listIndex = frameTasks.Count,
            updateAction = updateAction,
            endAction = endAction
        };

        frameTasks.Add(task);
        frameTaskDic[task.id] = task;
        return handle;
    }

    public bool Pause(FrameTaskHandle handle)
    {
        if (!TryGetFrameTask(handle, out var task))
        {
            return false;
        }

        task.paused = true;
        return true;
    }

    public bool Resume(FrameTaskHandle handle)
    {
        if (!TryGetFrameTask(handle, out var task))
        {
            return false;
        }

        task.paused = false;
        return true;
    }

    public bool Cancel(FrameTaskHandle handle)
    {
        if (!TryGetFrameTask(handle, out var task))
        {
            return false;
        }

        RemoveFrameTaskAt(task.listIndex);
        return true;
    }

    public bool Complete(FrameTaskHandle handle)
    {
        if (!TryGetFrameTask(handle, out var task))
        {
            return false;
        }

        CompleteFrameTaskAt(task.listIndex);
        return true;
    }

    public MoveHandle StartLineMove(Vector3 startPos, Vector3 targetPos, float moveTime, CurveAction curveAction,
        CurveEndAction curveEndAction = null)
    {
        return AddTween(TweenType.Line, startPos, targetPos, Vector3.zero, 0f, moveTime, curveAction, curveEndAction,
            null, false);
    }

    public MoveHandle StartLineMoveBySpeed(float speed, Vector3 startPos, Vector3 targetPos, CurveAction curveAction,
        CurveEndAction curveEndAction = null)
    {
        return StartLineMove(startPos, targetPos, GetMoveTimeFromSpeed(speed), curveAction, curveEndAction);
    }

    public MoveHandle StartCurveMove(Vector3 startPos, Vector3 targetPos, Vector3 middlePos, float moveTime,
        CurveAction curveAction, CurveEndAction curveEndAction = null)
    {
        return AddTween(TweenType.Curve, startPos, targetPos, middlePos, 0f, moveTime, curveAction, curveEndAction,
            null, false);
    }

    public MoveHandle StartCurveMoveBySpeed(float speed, Vector3 startPos, Vector3 targetPos, Vector3 middlePos,
        CurveAction curveAction, CurveEndAction curveEndAction = null)
    {
        return StartCurveMove(startPos, targetPos, middlePos, GetMoveTimeFromSpeed(speed), curveAction, curveEndAction);
    }

    public MoveGroupHandle StartCurveMoveList(List<CurveMoveData> curveMoveDatas, CurveEndAction curveEndAction)
    {
        return AddMoveList(curveMoveDatas, TweenType.Curve, curveEndAction);
    }

    public MoveGroupHandle StartLineMoveList(List<CurveMoveData> curveMoveDatas, CurveEndAction curveEndAction)
    {
        return AddMoveList(curveMoveDatas, TweenType.Line, curveEndAction);
    }

    public bool Pause(MoveHandle handle)
    {
        if (!TryGetTween(handle, out var task))
        {
            return false;
        }

        task.paused = true;
        return true;
    }

    public bool Resume(MoveHandle handle)
    {
        if (!TryGetTween(handle, out var task))
        {
            return false;
        }

        task.paused = false;
        return true;
    }

    public bool Cancel(MoveHandle handle)
    {
        return CancelTween(handle, false);
    }

    public bool Pause(MoveGroupHandle handle)
    {
        if (!TryGetGroup(handle, out var group))
        {
            return false;
        }

        group.paused = true;
        return true;
    }

    public bool Resume(MoveGroupHandle handle)
    {
        if (!TryGetGroup(handle, out var group))
        {
            return false;
        }

        group.paused = false;
        return true;
    }

    public bool Cancel(MoveGroupHandle handle)
    {
        if (!TryGetGroup(handle, out var group))
        {
            return false;
        }

        for (int i = tweenTasks.Count - 1; i >= 0; i--)
        {
            if (tweenTasks[i].group == group)
            {
                RemoveTweenAt(i);
            }
        }

        RemoveGroup(group);
        return true;
    }

    public void StartCharacterObjectMove(Character character, GetVector2 getObjectPos, SetMoveTarge setMoveTarget,
        int instanceId)
    {
        if (objectMoveTaskDic.ContainsKey(instanceId))
        {
            return;
        }

        AddObjectMove(new ObjectMoveTask
        {
            instanceId = instanceId,
            moveType = ObjectMoveType.Character,
            character = character,
            getObjectPos = getObjectPos,
            setMoveTarget = setMoveTarget,
            speed = 1f
        });
    }

    public void StartFreedomObjectMove(GetVector2 getObjectPos, GetVector2 getMoveDirection,
        SetMoveTarge setMoveTarget, int mapId, int instanceId, bool checkWalk, float speed = 1)
    {
        if (objectMoveTaskDic.ContainsKey(instanceId))
        {
            return;
        }

        AddObjectMove(new ObjectMoveTask
        {
            instanceId = instanceId,
            moveType = ObjectMoveType.Freedom,
            getObjectPos = getObjectPos,
            getMoveDirection = getMoveDirection,
            setMoveTarget = setMoveTarget,
            mapId = mapId,
            checkWalk = checkWalk,
            speed = speed
        });
    }

    public void StopObjectMove(int instanceId)
    {
        if (!objectMoveTaskDic.TryGetValue(instanceId, out var task))
        {
            return;
        }

        RemoveObjectMoveAt(task.listIndex);
    }

    protected override void Update()
    {
        base.Update();

        float deltaTime = Time.deltaTime;
        UpdateFrameTasks(deltaTime);
        UpdateTweens(deltaTime);
        UpdateObjectMoves(deltaTime);
    }

    protected override void Clear()
    {
        CancelAllFrameTasks();
        CancelAllTweens();
        ClearObjectMoves();
        base.Clear();
    }

    private MoveGroupHandle AddMoveList(List<CurveMoveData> curveMoveDatas, TweenType tweenType,
        CurveEndAction curveEndAction)
    {
        var group = AddGroup(curveEndAction);
        var handle = new MoveGroupHandle(group.id, group.version);

        if (curveMoveDatas != null)
        {
            for (int i = 0; i < curveMoveDatas.Count; i++)
            {
                var curveMoveData = curveMoveDatas[i];
                if (curveMoveData == null || curveMoveData.transform == null)
                {
                    continue;
                }

                group.remainingCount++;
                AddTransformTween(curveMoveData, tweenType, group);
            }
        }

        if (group.remainingCount <= 0)
        {
            CompleteGroup(group);
        }

        return handle;
    }

    private MoveGroup AddGroup(CurveEndAction curveEndAction)
    {
        var group = new MoveGroup
        {
            id = ++nextMoveGroupId,
            version = ++nextMoveGroupVersion,
            listIndex = moveGroups.Count,
            endAction = curveEndAction
        };

        moveGroups.Add(group);
        moveGroupDic[group.id] = group;
        return group;
    }

    private void AddTransformTween(CurveMoveData curveMoveData, TweenType tweenType, MoveGroup group)
    {
        var handle = AddTween(tweenType, curveMoveData.startPos, curveMoveData.targetPos, curveMoveData.middlePos,
            Mathf.Max(0f, curveMoveData.waitTime), Mathf.Max(0f, curveMoveData.moveTime), null,
            curveMoveData.CurveEndAction, group, true);

        if (TryGetTween(handle, out var task))
        {
            task.transform = curveMoveData.transform;
        }
    }

    private MoveHandle AddTween(TweenType tweenType, Vector3 startPos, Vector3 targetPos, Vector3 middlePos,
        float waitTime, float moveTime, CurveAction curveAction, CurveEndAction endAction, MoveGroup group,
        bool scaleOnStart)
    {
        var handle = new MoveHandle(++nextMoveId, ++nextMoveVersion);
        var task = new TweenTask
        {
            id = handle.id,
            version = handle.version,
            listIndex = tweenTasks.Count,
            waitTime = Mathf.Max(0f, waitTime),
            moveTime = Mathf.Max(0f, moveTime),
            tweenType = tweenType,
            startPos = startPos,
            targetPos = targetPos,
            middlePos = middlePos,
            curveAction = curveAction,
            endAction = endAction,
            group = group,
            scaleOnStart = scaleOnStart
        };

        tweenTasks.Add(task);
        tweenTaskDic[task.id] = task;
        return handle;
    }

    private bool TryGetTween(MoveHandle handle, out TweenTask task)
    {
        if (!handle.IsValid ||
            !tweenTaskDic.TryGetValue(handle.id, out task) ||
            task.version != handle.version)
        {
            task = null;
            return false;
        }

        return true;
    }

    private bool TryGetGroup(MoveGroupHandle handle, out MoveGroup group)
    {
        if (!handle.IsValid ||
            !moveGroupDic.TryGetValue(handle.id, out group) ||
            group.version != handle.version)
        {
            group = null;
            return false;
        }

        return true;
    }

    private void UpdateTweens(float deltaTime)
    {
        // 倒序遍历配合 swap remove，完成或取消任务时不会移动大量元素。
        for (int i = tweenTasks.Count - 1; i >= 0; i--)
        {
            var task = tweenTasks[i];
            if (task.paused || IsGroupPaused(task.group))
            {
                continue;
            }

            task.elapsedTime += deltaTime;
            if (task.elapsedTime < task.waitTime)
            {
                continue;
            }

            float timeValue = GetTweenTimeValue(task);
            bool invokeEndAction = UpdateTweenPosition(task, timeValue);

            if (timeValue >= 1f)
            {
                CompleteTweenAt(i, invokeEndAction);
            }
        }
    }

    private bool UpdateTweenPosition(TweenTask task, float timeValue)
    {
        Vector3 pos = task.tweenType == TweenType.Curve
            ? GetCurvePosition(task.startPos, task.middlePos, task.targetPos, timeValue)
            : Vector3.LerpUnclamped(task.startPos, task.targetPos, timeValue);

        if (task.transform != null)
        {
            if (task.scaleOnStart && !task.scaleApplied)
            {
                task.transform.localScale = Vector3.one;
                task.scaleApplied = true;
            }

            task.transform.position = pos;
            return true;
        }

        if (task.curveAction != null)
        {
            task.curveAction(pos);
            return true;
        }

        // Transform 被外部销毁时，不再调用单项完成回调，避免回收已失效对象。
        return false;
    }

    private float GetTweenTimeValue(TweenTask task)
    {
        if (task.moveTime <= 0f)
        {
            return 1f;
        }

        return Mathf.Clamp01((task.elapsedTime - task.waitTime) / task.moveTime);
    }

    private bool IsGroupPaused(MoveGroup group)
    {
        return group != null && group.paused;
    }

    private void CompleteTweenAt(int index, bool invokeEndAction)
    {
        var task = tweenTasks[index];
        RemoveTweenAt(index);

        if (invokeEndAction && task.endAction != null)
        {
            try
            {
                task.endAction.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        CompleteTweenGroup(task.group);
    }

    private bool CancelTween(MoveHandle handle, bool completeGroup)
    {
        if (!TryGetTween(handle, out var task))
        {
            return false;
        }

        RemoveTweenAt(task.listIndex);

        if (completeGroup)
        {
            CompleteTweenGroup(task.group);
        }

        return true;
    }

    private void RemoveTweenAt(int index)
    {
        int lastIndex = tweenTasks.Count - 1;
        var task = tweenTasks[index];

        tweenTaskDic.Remove(task.id);

        tweenTasks[index] = tweenTasks[lastIndex];
        tweenTasks[index].listIndex = index;
        tweenTasks.RemoveAt(lastIndex);
    }

    private void CompleteTweenGroup(MoveGroup group)
    {
        if (group == null)
        {
            return;
        }

        group.remainingCount--;
        if (group.remainingCount <= 0)
        {
            CompleteGroup(group);
        }
    }

    private void CompleteGroup(MoveGroup group)
    {
        RemoveGroup(group);

        if (group.endAction == null)
        {
            return;
        }

        try
        {
            group.endAction.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void RemoveGroup(MoveGroup group)
    {
        if (group == null || !moveGroupDic.ContainsKey(group.id))
        {
            return;
        }

        int index = group.listIndex;
        int lastIndex = moveGroups.Count - 1;

        moveGroupDic.Remove(group.id);
        moveGroups[index] = moveGroups[lastIndex];
        moveGroups[index].listIndex = index;
        moveGroups.RemoveAt(lastIndex);
    }

    private void CancelAllTweens()
    {
        tweenTasks.Clear();
        tweenTaskDic.Clear();
        moveGroups.Clear();
        moveGroupDic.Clear();
    }

    private bool TryGetFrameTask(FrameTaskHandle handle, out FrameTask task)
    {
        if (!handle.IsValid ||
            !frameTaskDic.TryGetValue(handle.id, out task) ||
            task.version != handle.version)
        {
            task = null;
            return false;
        }

        return true;
    }

    private void UpdateFrameTasks(float deltaTime)
    {
        for (int i = frameTasks.Count - 1; i >= 0; i--)
        {
            var task = frameTasks[i];
            if (task.paused)
            {
                continue;
            }

            bool keepRunning;
            try
            {
                keepRunning = task.updateAction(deltaTime);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                keepRunning = false;
            }

            if (!keepRunning)
            {
                CompleteFrameTaskAt(i);
            }
        }
    }

    private void CompleteFrameTaskAt(int index)
    {
        var task = frameTasks[index];
        RemoveFrameTaskAt(index);

        if (task.endAction == null)
        {
            return;
        }

        try
        {
            task.endAction.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void RemoveFrameTaskAt(int index)
    {
        int lastIndex = frameTasks.Count - 1;
        var task = frameTasks[index];
        frameTaskDic.Remove(task.id);

        frameTasks[index] = frameTasks[lastIndex];
        frameTasks[index].listIndex = index;
        frameTasks.RemoveAt(lastIndex);
    }

    private void CancelAllFrameTasks()
    {
        frameTasks.Clear();
        frameTaskDic.Clear();
    }

    private void AddObjectMove(ObjectMoveTask task)
    {
        task.listIndex = objectMoveTasks.Count;
        objectMoveTasks.Add(task);
        objectMoveTaskDic[task.instanceId] = task;
    }

    private void UpdateObjectMoves(float deltaTime)
    {
        // 连续移动统一在 Update 中推进，避免每个对象各自维护一个无限协程。
        for (int i = objectMoveTasks.Count - 1; i >= 0; i--)
        {
            var task = objectMoveTasks[i];
            try
            {
                bool keepMove = task.moveType == ObjectMoveType.Character
                    ? UpdateCharacterObjectMove(task, deltaTime)
                    : UpdateFreedomObjectMove(task, deltaTime);

                if (!keepMove)
                {
                    RemoveObjectMoveAt(i);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                RemoveObjectMoveAt(i);
            }
        }
    }

    private bool UpdateCharacterObjectMove(ObjectMoveTask task, float deltaTime)
    {
        if (task.character == null || task.getObjectPos == null || task.setMoveTarget == null)
        {
            return false;
        }

        if (task.character.moveDirection.Equals(float2.zero))
        {
            return false;
        }

        Vector2 direction = task.character.moveDirection;
        Vector3 nowPos = task.getObjectPos();
        float speed = task.character.propertySpeed;

        Vector3 targetPos = nowPos +
                            new Vector3(direction.x, direction.y, 0) * speed * GameCommon.freedomMoveValue * 1.1f *
                            deltaTime;
        targetPos = GameCommon.SetMapPosZ(targetPos);
        int2 targetCoordinate = GameCommon.GetMapCoordinateInt(targetPos);
        int2 trueTargetCoordinate =
            MapCellController.instance.GetTrueFreedomTarget(task.character.coordinate, targetCoordinate,
                task.character.mapInstance);

        if (!trueTargetCoordinate.Equals(targetCoordinate))
        {
            targetPos = !trueTargetCoordinate.Equals(task.character.coordinate)
                ? GameCommon.GetMapPos(trueTargetCoordinate)
                : nowPos;
        }

        task.setMoveTarget(trueTargetCoordinate, targetPos);
        return true;
    }

    private bool UpdateFreedomObjectMove(ObjectMoveTask task, float deltaTime)
    {
        if (task.getObjectPos == null || task.getMoveDirection == null || task.setMoveTarget == null)
        {
            return false;
        }

        Vector2 direction = task.getMoveDirection();
        Vector3 nowPos = task.getObjectPos();

        if (task.checkWalk)
        {
            int2 target = int2.zero;
            Vector3 targetPos = nowPos;
            float distance = CharacterManager.updataMoveSpeed * deltaTime * task.speed;

            if (!WorldMapManager.instance.InitSmoothMove(ref direction, nowPos, task.mapId, distance, ref target,
                    ref targetPos))
            {
                return false;
            }

            task.setMoveTarget(target, targetPos);
            return true;
        }

        Vector3 freeTargetPos = nowPos +
                                new Vector3(direction.x, direction.y, 0) * CharacterManager.updataMoveSpeed *
                                deltaTime * task.speed;
        freeTargetPos = GameCommon.SetMapPosZ(freeTargetPos);
        int2 targetCoordinate = GameCommon.GetMapCoordinateInt(freeTargetPos);
        task.setMoveTarget(targetCoordinate, freeTargetPos);
        return true;
    }

    private void RemoveObjectMoveAt(int index)
    {
        int lastIndex = objectMoveTasks.Count - 1;
        var task = objectMoveTasks[index];
        objectMoveTaskDic.Remove(task.instanceId);

        objectMoveTasks[index] = objectMoveTasks[lastIndex];
        objectMoveTasks[index].listIndex = index;
        objectMoveTasks.RemoveAt(lastIndex);
    }

    private void ClearObjectMoves()
    {
        objectMoveTasks.Clear();
        objectMoveTaskDic.Clear();
    }

    private float GetMoveTimeFromSpeed(float speed)
    {
        if (speed <= 0f)
        {
            Debug.LogError($"GameObjectCurveController speed 无效:{speed}");
            return 0f;
        }

        return 1f / speed;
    }

    private Vector3 GetCurvePosition(Vector3 startPos, Vector3 middlePos, Vector3 targetPos, float timeValue)
    {
        float oneMinusTime = 1f - timeValue;
        return oneMinusTime * oneMinusTime * startPos +
               2f * timeValue * oneMinusTime * middlePos +
               timeValue * timeValue * targetPos;
    }
}
