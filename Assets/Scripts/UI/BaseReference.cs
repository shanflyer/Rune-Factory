using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class BaseReference : MonoBehaviour
{
    public static LayerMask UILayer;
    public static LayerMask HideLayer;
    public static LayerMask WorldLayer;

    [SerializeField]
    public Canvas canvas;
    [SerializeField]
    public GraphicRaycaster raycaster;
    public bool show;
    [HideInInspector]
    public int index;
    private CancellationToken lifecycleCancellationToken = CancellationToken.None;
    public CancellationToken LifecycleCancellationToken => lifecycleCancellationToken;
    public virtual bool changeInputModel { get=>true; }
    protected bool LifecycleCanceled => lifecycleCancellationToken.IsCancellationRequested || SingletonType.Cleared || this == null;
    public virtual void SetPanelUISerializeObj()
    {
        gameObject.TryGetComponent(out canvas);
        gameObject.TryGetComponent(out raycaster);
    }
    public virtual void Show(int layer = -1) { show = true; }
    public virtual void Close()
    {
        show = false;
        CancelLifecycleTasks();
    }
    public virtual Task InitData(string dataKey)
    {
        // 默认面板没有异步数据，派生类可覆盖为真正的加载流程。
        return Task.CompletedTask;
    }

    public virtual Task InitData(string dataKey, CancellationToken cancellationToken)
    {
        // UIManager 会在新打开或关闭面板时取消旧 token，派生类可读取该 token 中断更细的异步刷新。
        SetLifecycleCancellationToken(cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }
        return InitData(dataKey);
    }

    public void SetLifecycleCancellationToken(CancellationToken cancellationToken)
    {
        lifecycleCancellationToken = cancellationToken;
    }

    protected void CancelLifecycleTasks()
    {
        // 有些面板会直接调用 Close，不经过 UIManager.CloseGamePanel，这里兜底取消旧异步回调。
        lifecycleCancellationToken = new CancellationToken(true);
    }

    protected bool ShouldStopLifecycleTask(CancellationToken cancellationToken)
    {
        return cancellationToken.IsCancellationRequested || lifecycleCancellationToken.IsCancellationRequested ||
               SingletonType.Cleared || this == null;
    }

    protected void RunLifecycleTask(Func<CancellationToken, Task> taskFactory, string context)
    {
        var cancellationToken = lifecycleCancellationToken;
        AsyncTaskRunner.Run(async () =>
        {
            if (ShouldStopLifecycleTask(cancellationToken))
            {
                return;
            }

            await taskFactory(cancellationToken);
        }, context);
    }

    protected virtual void OnDestroy()
    {
        if (SingletonType.Cleared || !GameActionManager.HasInstance)
        {
            return;
        }

        // UI 或子引用被销毁时清掉以当前对象为目标的全局监听，避免旧回调跨场景残留。
        GameActionManager.instance.RemoveListenersForTarget(this);
    }

}
