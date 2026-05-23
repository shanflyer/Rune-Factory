using System;
using System.Collections.Generic;
using UnityEngine;

public class GameTimerController : Singleton<GameTimerController>
{
    public struct TimerHandle
    {
        public readonly int id;
        public readonly int version;

        public TimerHandle(int id, int version)
        {
            this.id = id;
            this.version = version;
        }

        public bool IsValid => id > 0;
    }

    private class TimerTask
    {
        public int id;
        public int version;
        public float remainingTime;
        public Action action;
        public object key;
        public bool useUnscaledTime;
        public bool cancelled;
    }

    private readonly List<TimerTask> timerTasks = new List<TimerTask>();
    private readonly Dictionary<int, TimerTask> timerTaskDic = new Dictionary<int, TimerTask>();
    private readonly Dictionary<object, TimerHandle> keyedTimers = new Dictionary<object, TimerHandle>();
    private int nextTimerId;
    private int nextTimerVersion;

    public override bool NeedUpdate => true;

    public override void Init()
    {
        base.Init();
    }

    protected override void Clear()
    {
        CancelAll();
        base.Clear();
    }

    public void RemoveWaiter(Action action)
    {
        CancelByKey(action);
    }

    public void DelayAction(int delay, Action action)
    {
        if (action == null)
        {
            return;
        }

        if (GameDataManager.instance.GlobalData.debug)
        {
            Debug.Log($"target:{action.Target}-Method:{action.Method}");
        }

        DelayOrReplace(action, delay * 0.001f, action);
    }

    public TimerHandle Delay(float delaySeconds, Action action, bool useUnscaledTime = false)
    {
        return AddTimer(null, delaySeconds, action, useUnscaledTime);
    }

    public TimerHandle DelayOrReplace(object key, float delaySeconds, Action action, bool useUnscaledTime = false)
    {
        if (key == null)
        {
            return Delay(delaySeconds, action, useUnscaledTime);
        }

        CancelByKey(key);
        return AddTimer(key, delaySeconds, action, useUnscaledTime);
    }

    public bool Cancel(TimerHandle handle)
    {
        if (!handle.IsValid)
        {
            return false;
        }

        if (!timerTaskDic.TryGetValue(handle.id, out var task) || task.version != handle.version)
        {
            return false;
        }

        task.cancelled = true;
        timerTaskDic.Remove(handle.id);
        RemoveKey(task);
        return true;
    }

    public bool CancelByKey(object key)
    {
        if (key == null)
        {
            return false;
        }

        if (!keyedTimers.TryGetValue(key, out var handle))
        {
            return false;
        }

        return Cancel(handle);
    }

    public void CancelAll()
    {
        for (int i = 0; i < timerTasks.Count; i++)
        {
            timerTasks[i].cancelled = true;
        }

        timerTasks.Clear();
        timerTaskDic.Clear();
        keyedTimers.Clear();
    }

    private TimerHandle AddTimer(object key, float delaySeconds, Action action, bool useUnscaledTime)
    {
        if (action == null)
        {
            return default;
        }

        var handle = new TimerHandle(++nextTimerId, ++nextTimerVersion);
        var task = new TimerTask
        {
            id = handle.id,
            version = handle.version,
            remainingTime = Mathf.Max(0f, delaySeconds),
            action = action,
            key = key,
            useUnscaledTime = useUnscaledTime
        };

        timerTasks.Add(task);
        timerTaskDic.Add(task.id, task);

        if (key != null)
        {
            keyedTimers[key] = handle;
        }

        return handle;
    }

    private void RemoveKey(TimerTask task)
    {
        if (task.key == null)
        {
            return;
        }

        if (keyedTimers.TryGetValue(task.key, out var handle) &&
            handle.id == task.id &&
            handle.version == task.version)
        {
            keyedTimers.Remove(task.key);
        }
    }

    private void RemoveTaskAt(int index)
    {
        int lastIndex = timerTasks.Count - 1;
        timerTasks[index] = timerTasks[lastIndex];
        timerTasks.RemoveAt(lastIndex);
    }

    protected override void Update()
    {
        base.Update();

        float deltaTime = Time.deltaTime;
        float unscaledDeltaTime = Time.unscaledDeltaTime;

        for (int i = timerTasks.Count - 1; i >= 0; i--)
        {
            var task = timerTasks[i];
            if (task.cancelled)
            {
                RemoveTaskAt(i);
                continue;
            }

            task.remainingTime -= task.useUnscaledTime ? unscaledDeltaTime : deltaTime;
            if (task.remainingTime > 0f)
            {
                continue;
            }

            timerTaskDic.Remove(task.id);
            RemoveKey(task);
            RemoveTaskAt(i);

            try
            {
                task.action.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
