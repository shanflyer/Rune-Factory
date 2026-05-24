using System;
using System.Collections.Generic;
using UnityEngine;

public class GameActionManager : Singleton<GameActionManager>
{
    private const int MaxImmediateActionDepth = 64;
    private const int MaxQueuedActionsPerFrame = 10000;

    public override bool NeedUpdate { get => true; }

    public delegate void ActionDelegate<T>(T e) where T : GameAction;

    public delegate void ActionBus();

    private Queue<ActionBus> ActionQueue = new Queue<ActionBus>();
    private Dictionary<Type, Delegate> delegates = new Dictionary<Type, Delegate>();
    private Dictionary<Delegate, Delegate> asyncDelegateWrappers = new Dictionary<Delegate, Delegate>();
    private HashSet<Delegate> onceDelegates = new HashSet<Delegate>();
    private int immediateActionDepth;

    public override void Init()
    {
        base.Init();
        delegates.Clear();
        asyncDelegateWrappers.Clear();
        onceDelegates.Clear();
    }

    protected override void Clear()
    {
        base.Clear();
        delegates.Clear();
        asyncDelegateWrappers.Clear();
        onceDelegates.Clear();
        ActionQueue.Clear();
    }

    public void AddListener<T>(ActionDelegate<T> del, bool once = false) where T : GameAction
    {
        Type type = typeof(T);
        if (delegates.TryGetValue(type, out Delegate d))
        {
            var _d = d as ActionDelegate<T>;
            if (_d == null)
            {
                delegates[type] = del;
            }
            else if (!ContainsDelegate(_d, del))
            {
                // 场景或行为树可能重复初始化，同一委托只注册一次，避免事件被重复响应。
                _d += del;
                delegates[type] = _d;
            }
        }
        else
        {
            delegates[type] = del;
        }
        if (once)
        {
            onceDelegates.Add(del);
        }
    }

    public void AddAsyncListener<T>(Func<T, System.Threading.Tasks.Task> del, string context, bool once = false) where T : GameAction
    {
        // Action 总线仍保持同步派发，异步监听器通过统一入口承接异常。
        if (!asyncDelegateWrappers.TryGetValue(del, out var wrapperDelegate))
        {
            ActionDelegate<T> wrapper = action => AsyncTaskRunner.Run(() => del(action), context);
            asyncDelegateWrappers[del] = wrapper;
            wrapperDelegate = wrapper;
        }

        AddListener<T>((ActionDelegate<T>)wrapperDelegate, once);
    }

    public void RemoveAsyncListener<T>(Func<T, System.Threading.Tasks.Task> del) where T : GameAction
    {
        if (!asyncDelegateWrappers.TryGetValue(del, out var wrapperDelegate))
        {
            return;
        }

        RemoveListener<T>((ActionDelegate<T>)wrapperDelegate);
        asyncDelegateWrappers.Remove(del);
    }

    public void RemoveListenersForTarget(object target)
    {
        if (target == null)
        {
            return;
        }

        RemoveAsyncWrappersForTarget(target);

        var types = new List<Type>(delegates.Keys);
        for (int i = 0; i < types.Count; i++)
        {
            Type type = types[i];
            if (!delegates.TryGetValue(type, out var source))
            {
                continue;
            }

            Delegate kept = null;
            foreach (var item in source.GetInvocationList())
            {
                if (item.Target == target)
                {
                    onceDelegates.Remove(item);
                    continue;
                }

                kept = kept == null ? item : Delegate.Combine(kept, item);
            }

            if (kept == null)
            {
                delegates.Remove(type);
            }
            else
            {
                delegates[type] = kept;
            }
        }
    }

    private void RemoveAsyncWrappersForTarget(object target)
    {
        var asyncDelegates = new List<Delegate>(asyncDelegateWrappers.Keys);
        for (int i = 0; i < asyncDelegates.Count; i++)
        {
            Delegate asyncDelegate = asyncDelegates[i];
            if (asyncDelegate.Target != target)
            {
                continue;
            }

            if (asyncDelegateWrappers.TryGetValue(asyncDelegate, out var wrapper))
            {
                RemoveWrappedDelegate(wrapper);
            }

            asyncDelegateWrappers.Remove(asyncDelegate);
        }
    }

    private void RemoveWrappedDelegate(Delegate wrapper)
    {
        var types = new List<Type>(delegates.Keys);
        for (int i = 0; i < types.Count; i++)
        {
            Type type = types[i];
            if (!delegates.TryGetValue(type, out var source))
            {
                continue;
            }

            Delegate kept = null;
            foreach (var item in source.GetInvocationList())
            {
                if (item == wrapper)
                {
                    onceDelegates.Remove(item);
                    continue;
                }

                kept = kept == null ? item : Delegate.Combine(kept, item);
            }

            if (kept == null)
            {
                delegates.Remove(type);
            }
            else
            {
                delegates[type] = kept;
            }
        }
    }

    private static bool ContainsDelegate<T>(ActionDelegate<T> source, ActionDelegate<T> target) where T : GameAction
    {
        foreach (var item in source.GetInvocationList())
        {
            if (item == (Delegate)target)
            {
                return true;
            }
        }

        return false;
    }

    public void RemoveListener<T>(ActionDelegate<T> del) where T : GameAction
    {
        Type type = typeof(T);
        if (delegates.TryGetValue(type, out Delegate d))
        {
            var _d = d as ActionDelegate<T>;
            _d -= del;
            if (_d == null)
            {
                delegates.Remove(type);
            }
            else
            {
                delegates[type] = _d;
            }
            onceDelegates.Remove(del);
        }
    }

    public void TriggerAction<T>(T gameAction) where T : GameAction
    {
        Type type = typeof(T);
        if (delegates.TryGetValue(type, out Delegate d))
        {
            var _d = d as ActionDelegate<T>;
            if (_d == null)
            {
                delegates.Remove(type);
                return;
            }
            if (immediateActionDepth >= MaxImmediateActionDepth)
            {
                Debug.LogError($"GameActionManager trigger depth exceeded: type={type.FullName}, depth={immediateActionDepth}");
                return;
            }

            immediateActionDepth++;
            try
            {
                foreach (Delegate _delegate in d.GetInvocationList())
                {
                    var actionDelegate = _delegate as ActionDelegate<T>;
                    if (actionDelegate == null)
                    {
                        continue;
                    }

                    try
                    {
                        // 单个监听失败不能阻断同一 Action 的其他监听，once 清理也必须继续执行。
                        actionDelegate(gameAction);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"GameActionManager listener failed: type={type.FullName}, listener={_delegate.Method.DeclaringType?.FullName}.{_delegate.Method.Name}");
                        Debug.LogException(e);
                    }

                    if (onceDelegates.Contains(_delegate))
                    {
                        _d -= actionDelegate;
                        if (_d == null)
                        {
                            delegates.Remove(type);
                        }
                        else
                        {
                            delegates[type] = _d;
                        }
                        onceDelegates.Remove(_delegate);
                    }
                }
            }
            finally
            {
                immediateActionDepth--;
            }
        }
    }

    public void QueueAction<T>(T gameAction, bool immediately = false) where T : GameAction
    { 
        if (GameDataManager.instance!=null&& GameDataManager.instance.GlobalData !=null&& GameDataManager.instance.GlobalData.immediatelyAction)
        {
            TriggerAction(gameAction); return;
        } 
        if (immediately)
        {
            TriggerAction(gameAction);
        } 
        else
        {
            Type type = typeof(T);
            if (delegates.ContainsKey(type))
            {
                ActionQueue.Enqueue(() =>
                {
                    TriggerAction(gameAction);
                });
            }
        } 
    }

    protected override void Update()
    {
        int executedCount = 0;
        while (ActionQueue.Count > 0)
        {
            var gameAction = ActionQueue.Dequeue();
            if (gameAction != null)
            {
                try
                {
                    gameAction();
                }
                catch (Exception e)
                {
                    Debug.LogError("GameActionManager queued action failed.");
                    Debug.LogException(e);
                }
            }

            executedCount++;
            if (executedCount >= MaxQueuedActionsPerFrame)
            {
                // 防止循环派发在同一帧无限扩张，剩余 Action 留到下一帧继续处理。
                Debug.LogError($"GameActionManager queued action limit reached: count={executedCount}, remaining={ActionQueue.Count}");
                break;
            }
        }
    }
}
