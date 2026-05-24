using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IStartupManager
{
    Type ManagerType { get; }
    string ManagerName { get; }
    IReadOnlyList<Type> InitializationDependencies { get; }
    Task WaitForInitialization();
}

public class Singleton<T> : IStartupManager where T : Singleton<T>
{
    public static T instance
    {
        get
        {
            if (Application.isPlaying)
            {
                if (_instance == null && !SingletonType.Cleared)
                {
                    _instance = Activator.CreateInstance<T>();
                    _instance.Init();
                    if (typeof(T) != typeof(SingletonType))
                    {
                        SingletonType.instance.AddType(_instance.Clear);

                        if (_instance.NeedUpdate)
                        {
                            SingletonType.instance.AddUpdateAction(_instance.Update);
                        }
                        if (_instance.NeedLateUpdate)
                        {
                            SingletonType.instance.AddLateUpdateAction(_instance.LateUpdate);
                        }

                        if (_instance.NeedFixedUpdate)
                            SingletonType.instance.AddFixedUpdateAction(_instance.FixedUpdate);
                    }
                }
            }
            else
            {
                if (_instance == null)
                {
                    _instance = Activator.CreateInstance<T>();
                    _instance.Init();

                }
            }

            return _instance;
        }
    }
    private static T _instance;
    public static bool HasInstance => _instance != null;

    public virtual bool NeedUpdate
    {
        get;
    }

    public virtual bool NeedFixedUpdate { get; }
    public virtual bool NeedLateUpdate
    {
        get;
    }

    public Type ManagerType => typeof(T);
    public string ManagerName => typeof(T).Name;

    protected virtual void FixedUpdate()
    {
    }
    protected virtual void Update()
    {

    }
    protected virtual void LateUpdate()
    {

    }
    public virtual void Init()
    {

    }

    public virtual Task InitializationTask => Task.CompletedTask;

    public virtual IReadOnlyList<Type> InitializationDependencies => Array.Empty<Type>();

    public virtual async Task WaitForInitialization()
    {
        await InitializationTask;
    }

    protected virtual void Clear()
    {
        if (typeof(T) != typeof(GameActionManager))
        {
            // Manager 清理时按实例目标移除全局 Action 监听，避免场景重载后旧实例重复响应。
            GameActionManager.instance?.RemoveListenersForTarget(_instance);
        }
        if (NeedUpdate)
        {
            SingletonType.instance.RemoveUpdateAction(_instance.Update);
        }
        if (NeedLateUpdate)
        {
            SingletonType.instance.RemoveLateUpdateAction(_instance.LateUpdate);
        }

        if (NeedFixedUpdate) SingletonType.instance.RemoveFixedUpdateAction(_instance.FixedUpdate);
        _instance = null;
    }
}
public delegate void SingletonClear();
public class SingletonType : Singleton<SingletonType>
{
    public HashSet<SingletonClear> TypeClears = new HashSet<SingletonClear>();
    public List<Action> singleUpdates = new List<Action>();
    public List<Action> singleFixedUpdates = new();
    public List<Action> singleLateUpdates = new List<Action>();
    public override void Init()
    {
        base.Init();
        Cleared = false;
    }
    public void AddUpdateAction(Action action)
    {
        if (!singleUpdates.Contains(action))
        {
            singleUpdates.Add(action);
        }
    }

    public void AddFixedUpdateAction(Action action)
    {
        if (!singleFixedUpdates.Contains(action)) singleFixedUpdates.Add(action);
    }

    public void RemoveFixedUpdateAction(Action action)
    {
        if (singleFixedUpdates.Contains(action)) singleFixedUpdates.Remove(action);
    }
    public void RemoveUpdateAction(Action action)
    {
        if (singleUpdates.Contains(action))
        {
            singleUpdates.Remove(action);
        }
    }
    public void AddLateUpdateAction(Action action)
    {
        if (!singleLateUpdates.Contains(action))
        {
            singleLateUpdates.Add(action);
        }
    }
    public void RemoveLateUpdateAction(Action action)
    {
        if (singleLateUpdates.Contains(action))
        {
            singleLateUpdates.Remove(action);
        }
    }
    public void AddType(SingletonClear typeClear)
    {
        TypeClears.Add(typeClear);
    }
    public static bool Cleared { get; private set; }
    public void ClearAll()
    {
        Cleared = true;
        // 场景或存档重载时先取消未完成异步任务，避免旧流程在 Manager 清理后继续回写状态。
        AsyncTaskRunner.CancelAll();
        singleUpdates.Clear();
        foreach (var typeClear in TypeClears)
        {
            try
            {
                if (typeClear != null)
                {
                    typeClear();
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }

        TypeClears.Clear();
        Clear();
    }

    public new void Update()
    {
        for(int i = 0; i < singleUpdates.Count; i++)
        {
            singleUpdates[i].Invoke();
        }
    }

    public new void FixedUpdate()
    {
        for (var i = 0; i < singleFixedUpdates.Count; i++) singleFixedUpdates[i].Invoke();
    }
    public new void LateUpdate()
    {
        for (int i = 0; i < singleLateUpdates.Count; i++)
        {
            singleLateUpdates[i].Invoke();
        }
    }
}
