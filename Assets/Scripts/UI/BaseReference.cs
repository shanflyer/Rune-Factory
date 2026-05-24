using System.Collections;
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
    public virtual void SetPanelUISerializeObj()
    {
        gameObject.TryGetComponent(out canvas);
        gameObject.TryGetComponent(out raycaster);
    }
    public virtual void Show(int layer = -1) { show = true; }
    public virtual void Close() { show = false; } 
    public virtual Task InitData(string dataKey)
    {
        // 默认面板没有异步数据，派生类可覆盖为真正的加载流程。
        return Task.CompletedTask;
    }

    public virtual Task InitData(string dataKey, CancellationToken cancellationToken)
    {
        // UIManager 会在新打开或关闭面板时取消旧 token，派生类可读取该 token 中断更细的异步刷新。
        lifecycleCancellationToken = cancellationToken;
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }
        return InitData(dataKey);
    }

}
