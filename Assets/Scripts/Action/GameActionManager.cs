using System;
using System.Collections.Generic;
using UnityEngine;

public class GameActionManager : Singleton<GameActionManager>
{
    private const int MaxImmediateActionDepth = 64;
    private const int MaxImmediateSameTypeDepth = 16;
    private const int MaxQueuedActionsTotal = 20000;
    private const int MaxQueuedActionsPerFrame = 10000;

    public override bool NeedUpdate { get => true; }

    public delegate void ActionDelegate<T>(T e) where T : GameAction;

    public delegate void ActionBus();

    private readonly struct QueuedAction
    {
        public readonly Type actionType;
        public readonly ActionBus action;

        public QueuedAction(Type actionType, ActionBus action)
        {
            this.actionType = actionType;
            this.action = action;
        }
    }

    private Queue<QueuedAction> ActionQueue = new Queue<QueuedAction>();
    private Dictionary<Type, Delegate> delegates = new Dictionary<Type, Delegate>();
    private Dictionary<(Type actionType, Delegate listener), Delegate> asyncDelegateWrappers = new Dictionary<(Type actionType, Delegate listener), Delegate>();
    private HashSet<Delegate> onceDelegates = new HashSet<Delegate>();
    private Dictionary<Type, int> immediateActionTypeDepths = new Dictionary<Type, int>();
    private int immediateActionDepth;

    public override void Init()
    {
        base.Init();
        delegates.Clear();
        asyncDelegateWrappers.Clear();
        onceDelegates.Clear();
        immediateActionTypeDepths.Clear();
        immediateActionDepth = 0;
    }

    protected override void Clear()
    {
        base.Clear();
        delegates.Clear();
        asyncDelegateWrappers.Clear();
        onceDelegates.Clear();
        immediateActionTypeDepths.Clear();
        immediateActionDepth = 0;
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
        var key = (typeof(T), (Delegate)del);
        // 异步监听按 Action 类型分开缓存，避免同一委托跨类型复用包装回调。
        if (!asyncDelegateWrappers.TryGetValue(key, out var wrapperDelegate))
        {
            ActionDelegate<T> wrapper = action => AsyncTaskRunner.Run(() => del(action), context);
            asyncDelegateWrappers[key] = wrapper;
            wrapperDelegate = wrapper;
        }

        AddListener<T>((ActionDelegate<T>)wrapperDelegate, once);
    }

    public void RemoveAsyncListener<T>(Func<T, System.Threading.Tasks.Task> del) where T : GameAction
    {
        var key = (typeof(T), (Delegate)del);
        if (!asyncDelegateWrappers.TryGetValue(key, out var wrapperDelegate))
        {
            return;
        }

        RemoveListener<T>((ActionDelegate<T>)wrapperDelegate);
        asyncDelegateWrappers.Remove(key);
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
        var asyncDelegates = new List<(Type actionType, Delegate listener)>(asyncDelegateWrappers.Keys);
        for (int i = 0; i < asyncDelegates.Count; i++)
        {
            var asyncDelegate = asyncDelegates[i];
            if (asyncDelegate.listener.Target != target)
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
            if (_d == null)
            {
                delegates.Remove(type);
                onceDelegates.Remove(del);
                return;
            }

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
        if (gameAction == null || SingletonType.Cleared)
        {
            return;
        }

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
            if (GetImmediateActionTypeDepth(type) >= MaxImmediateSameTypeDepth)
            {
                // 同类型递归通常意味着监听里又派发了自身，直接截断避免栈溢出或同帧死循环。
                Debug.LogError($"GameActionManager same action recursion exceeded: type={type.FullName}, depth={GetImmediateActionTypeDepth(type)}");
                return;
            }

            immediateActionDepth++;
            IncreaseImmediateActionTypeDepth(type);
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
                DecreaseImmediateActionTypeDepth(type);
                immediateActionDepth--;
            }
        }
    }

    public GameAction QueueAction(int actionId, bool immediately = false)
    {
        if (SingletonType.Cleared || GameDataManager.instance == null)
        {
            return default;
        }

        return GameDataManager.instance.GameAction(actionId, immediately: immediately);
    }

    public GameAction QueueActionAsset(
        int actionId,
        int source = 0,
        int target = 0,
        int value = -1,
        SetResult setResult = null,
        SetValue setValue = null,
        bool immediately = false)
    {
        if (SingletonType.Cleared || GameDataManager.instance == null)
        {
            return default;
        }

        return GameDataManager.instance.GameAction(actionId, source, target, value, setResult, setValue, immediately);
    }

    public void QueueAction<T>(T gameAction, bool immediately = false) where T : GameAction
    {
        if (gameAction == null || SingletonType.Cleared)
        {
            return;
        }

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
                if (ActionQueue.Count >= MaxQueuedActionsTotal)
                {
                    Debug.LogError($"GameActionManager queue overflow: type={type.FullName}, count={ActionQueue.Count}");
                    return;
                }

                ActionQueue.Enqueue(new QueuedAction(type, () =>
                {
                    TriggerAction(gameAction);
                }));
            }
        }
    }

    private int GetImmediateActionTypeDepth(Type type)
    {
        return immediateActionTypeDepths.TryGetValue(type, out var depth) ? depth : 0;
    }

    private void IncreaseImmediateActionTypeDepth(Type type)
    {
        immediateActionTypeDepths[type] = GetImmediateActionTypeDepth(type) + 1;
    }

    private void DecreaseImmediateActionTypeDepth(Type type)
    {
        int depth = GetImmediateActionTypeDepth(type) - 1;
        if (depth <= 0)
        {
            immediateActionTypeDepths.Remove(type);
        }
        else
        {
            immediateActionTypeDepths[type] = depth;
        }
    }

    protected override void Update()
    {
        int executedCount = 0;
        while (ActionQueue.Count > 0)
        {
            var queuedAction = ActionQueue.Dequeue();
            if (queuedAction.action != null)
            {
                try
                {
                    queuedAction.action();
                }
                catch (Exception e)
                {
                    Debug.LogError($"GameActionManager queued action failed: type={queuedAction.actionType?.FullName}");
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
