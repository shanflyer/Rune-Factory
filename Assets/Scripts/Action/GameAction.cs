using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public interface GameAction
{
    public void Clear();

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false);


    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
}

public delegate void SetPanelReference(BaseReference baseReference);

public delegate void SetValue(int value);
public delegate void SetFloatValue(float value);

public delegate void SetInt3Value(int3 value);

public delegate void SetResult(bool value);

public static class AsyncTaskRunner
{
    private static readonly object SyncRoot = new object();
    private static readonly HashSet<Task> RunningTasks = new HashSet<Task>();
    private static readonly Dictionary<string, CancellationTokenSource> LatestTaskTokens = new Dictionary<string, CancellationTokenSource>();
    private static readonly Dictionary<string, Task> SerialTasks = new Dictionary<string, Task>();
    private static CancellationTokenSource globalCancellationSource = new CancellationTokenSource();

    public static CancellationToken GlobalToken
    {
        get
        {
            lock (SyncRoot)
            {
                return globalCancellationSource.Token;
            }
        }
    }

    public static int RunningTaskCount
    {
        get
        {
            lock (SyncRoot)
            {
                return RunningTasks.Count;
            }
        }
    }

    public static void Run(Task task, string context)
    {
        if (task == null)
        {
            return;
        }

        Track(RunAsync(task, context));
    }

    public static void Run(Func<Task> taskFactory, string context)
    {
        try
        {
            Run(taskFactory(), context);
        }
        catch (Exception e)
        {
            // 异步任务创建阶段也可能抛错，统一记录，避免同步回调静默失败。
            Debug.LogError($"Async task failed before scheduling: {context}");
            Debug.LogException(e);
        }
    }

    public static void Run(Func<CancellationToken, Task> taskFactory, string context)
    {
        if (taskFactory == null)
        {
            return;
        }

        Run(() => taskFactory(GlobalToken), context);
    }

    public static void RunLatest(string key, Func<CancellationToken, Task> taskFactory, string context)
    {
        if (string.IsNullOrEmpty(key) || taskFactory == null)
        {
            return;
        }

        CancellationTokenSource cancellationTokenSource;
        lock (SyncRoot)
        {
            if (LatestTaskTokens.TryGetValue(key, out var oldTokenSource))
            {
                oldTokenSource.Cancel();
            }

            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(globalCancellationSource.Token);
            LatestTaskTokens[key] = cancellationTokenSource;
        }

        Track(RunLatestAsync(key, taskFactory, context, cancellationTokenSource));
    }

    public static void RunSerial(string key, Func<CancellationToken, Task> taskFactory, string context)
    {
        if (string.IsNullOrEmpty(key) || taskFactory == null)
        {
            return;
        }

        Task serialTask;
        lock (SyncRoot)
        {
            SerialTasks.TryGetValue(key, out var previousTask);
            serialTask = RunSerialAsync(previousTask ?? Task.CompletedTask, taskFactory, context, GlobalToken);
            SerialTasks[key] = serialTask;
        }

        Track(serialTask);
        Track(ClearSerialTaskAsync(key, serialTask));
    }

    public static void CancelAll()
    {
        List<CancellationTokenSource> latestTokens;
        lock (SyncRoot)
        {
            globalCancellationSource.Cancel();
            globalCancellationSource = new CancellationTokenSource();
            latestTokens = new List<CancellationTokenSource>(LatestTaskTokens.Values);
            LatestTaskTokens.Clear();
            SerialTasks.Clear();
        }

        for (int i = 0; i < latestTokens.Count; i++)
        {
            latestTokens[i].Cancel();
        }
    }

    private static async Task RunAsync(Task task, string context)
    {
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // 场景清理或 latest 任务被新任务替换时属于正常取消。
        }
        catch (Exception e)
        {
            // 同步回调里无法直接 await 的任务统一走这里，避免异步异常静默丢失。
            Debug.LogError($"Async task failed: {context}");
            Debug.LogException(e);
        }
    }

    private static async Task RunLatestAsync(string key, Func<CancellationToken, Task> taskFactory, string context,
        CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                await taskFactory(cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            Debug.LogError($"Async latest task failed: {context}");
            Debug.LogException(e);
        }
        finally
        {
            lock (SyncRoot)
            {
                if (LatestTaskTokens.TryGetValue(key, out var current) && current == cancellationTokenSource)
                {
                    LatestTaskTokens.Remove(key);
                }
            }

            cancellationTokenSource.Dispose();
        }
    }

    private static async Task RunSerialAsync(Task previousTask, Func<CancellationToken, Task> taskFactory, string context,
        CancellationToken cancellationToken)
    {
        try
        {
            await previousTask;
            cancellationToken.ThrowIfCancellationRequested();
            await taskFactory(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception e)
        {
            Debug.LogError($"Async serial task failed: {context}");
            Debug.LogException(e);
        }
    }

    private static async Task ClearSerialTaskAsync(string key, Task serialTask)
    {
        await serialTask;

        lock (SyncRoot)
        {
            if (SerialTasks.TryGetValue(key, out var current) && current == serialTask)
            {
                SerialTasks.Remove(key);
            }
        }
    }

    private static void Track(Task task)
    {
        lock (SyncRoot)
        {
            RunningTasks.Add(task);
        }

        _ = UntrackAsync(task);
    }

    private static async Task UntrackAsync(Task task)
    {
        try
        {
            await task;
        }
        finally
        {
            lock (SyncRoot)
            {
                RunningTasks.Remove(task);
            }
        }
    }
}

public static class GameActionAsyncRunner
{
    public static void Run(Task task, string actionName)
    {
        // GameAction.Init 仍是同步接口，异步初始化统一转给通用兜底器。
        AsyncTaskRunner.Run(task, $"GameAction async init: {actionName}");
    }
}

public struct PayEndAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }
    public void Clear() { this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct PlayCharacterTimeLine : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public string playName;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            characterId = int.Parse(parameters[0].value);
            playName = parameters[1].value;
        }
        if (source != 0)
        {
            characterId = source;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetFixedPlayerShaderPos : GameAction
{
    public bool fixedPos;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            fixedPos = bool.Parse(parameters[0].value);
        }
        else
        {
            fixedPos = source != 0;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetCameraPixelValue : GameAction
{
    public SetValue setValue { get; set; }
    public int pixelValue;
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            pixelValue = int.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetCameraConfiner2D : GameAction
{
    public bool enable;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            enable = bool.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RefreshMapCamera : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SetFixedCamera : GameAction
{
    public SetValue setValue { get; set; }
    public bool fixedCamera;
    public Vector3 fixedPos;
    public Vector3 offsetPos;
    public FlowCameraType flowCameraType;
    public int pixelValue;
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            fixedCamera = bool.Parse(parameters[0].value);
        }
        if (parameters.Count > 3)
        {
            fixedPos = new Vector3(float.Parse(parameters[1].value), float.Parse(parameters[2].value),
                float.Parse(parameters[3].value));
        }
        else
        {
            fixedPos.x = float.MinValue;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshGameSaveData : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

}

public struct NewHour : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

}
public struct NewDay : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

}
public struct RefreshShopLevel : GameAction
{
    public string shopName;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            shopName = parameters[0].value;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryVisitShop : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public string ShopName;
    public int CharacterId;
    public int ShopObjId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            ShopName = parameters[0].value;
        if (parameters.Count > 1)
            CharacterId = int.Parse(parameters[1].value);

        if (source != 0&&source!=int.MinValue)
        {
            CharacterId = source;
        }
        if (target != 0)
        {
            ShopObjId = target;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryGiveGiftOpenPackage : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int fromCharacterId;
    public int toCharacterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            fromCharacterId = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            toCharacterId = int.Parse(parameters[1].value);

        if (source != 0)
            fromCharacterId = source;
        if (target != 0)
            toCharacterId = target;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct GiveGift : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int giveCharacter, receiveCharacter;
    public int giftId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            giveCharacter = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            receiveCharacter = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            giftId = int.Parse(parameters[2].value);

        if (source != 0)
            giveCharacter = source;
        if (target != 0)
            receiveCharacter = target;
        if (value != 0)
            giftId = value;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SwitchAutoStore : GameAction
{
    public bool isAuto;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            isAuto = bool.Parse(parameters[0].value);
        }
        if (source != int.MinValue)
        {
            isAuto = source == 1;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetPlayerStoreOpen : GameAction
{
    public bool open;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            open = bool.Parse(parameters[0].value);
        }
        if (source != int.MinValue)
        {
            open = source == 1;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryBuyPlayerGood : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int characterId;
    public int storeCounterId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

}

public struct BuyPlayerGood : GameAction
{
    public SetValue setValue { get; set; }
    public int storeCounterId;
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            storeCounterId = int.Parse(parameters[0].value);
        }
        if (target != 0)
        {
            storeCounterId = target;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ShowCoin : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public Vector2 pos;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            pos.x = float.Parse(parameters[0].value);
            pos.y = float.Parse(parameters[1].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct UpdateGameTime : GameAction
{
    public int year, season, day;
    public int hour, minute;
    public int totalMinute;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetStoreCounterItem : GameAction, IReferenceData
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int storeCounterId;
    public int itemId;
    public int count;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 2)
        {
            storeCounterId = int.Parse(parameters[0].value);
            itemId = int.Parse(parameters[1].value);
            count = int.Parse(parameters[2].value);
        }
        if (source != 0)
        {
            storeCounterId = source;
        }
        if (target != 0)
        {
            itemId = target;
            count = -1;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StoreCounterSetSelectItemAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int targetObj;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        targetObj = target;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetStoreCounter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int storeCounterId;
    public int playerId;
    public int nullAction;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        nullAction = int.Parse(parameters[0].value);
        playerId = source;
        storeCounterId = target;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DisplayStoreCounter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public bool display;
    public int itemInstanceId;
    public Transform transform;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            display = bool.Parse(parameters[0].value);
            itemInstanceId = int.Parse(parameters[1].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct CreatStoreCounter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int itemInstanceId;
    public int storeDataId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 1)
        {
            itemInstanceId = int.Parse(parameters[0].value);
            storeDataId = int.Parse(parameters[1].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct PlayerTalkItem : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int displayTime;
    public int characterId;
    public int ItemId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
            displayTime = int.Parse(parameters[0].value);
        if (parameters.Count > 1)
            characterId = int.Parse(parameters[1].value);
        if (parameters.Count > 2)
            ItemId = int.Parse(parameters[2].value);

        characterId = source;
        ItemId = target;
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SwitchOperateList : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

}

public struct ShopBuySuccess : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int buyCount;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            buyCount = int.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct RefreshPackage : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int packageId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            packageId = int.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct AddPlayerGold : GameAction
{
    public int value;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            this.value = int.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct RefreshPlayerGold : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

}

public struct InitInputAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

}

public struct EndPlayerRound : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StopAutoFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct PlayerFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct FightCharacterMove : GameAction
{
    public int characterId;
    public int newIndex;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            characterId = int.Parse(parameters[0].value);
        }
        if (parameters.Count > 1)
        {
            newIndex = int.Parse(parameters[1].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StartRoundFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct WaitAction : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            int WaitValue = int.Parse(parameters[0].value);
            var _parameters = parameters[0].parameters;
            for (int i = 0; i < _parameters.Count; i++)
            {
                var Parameter = _parameters[i];
                if (GameDataManager.instance.GlobalData.debug)
                {
                    Debug.Log($"wait type:{Parameter.value}--Parameter:{Parameter}");
                }

                GameTimerController.instance.DelayAction(WaitValue, () =>
                {
                    GameActionDataManager.instance.GameAction(Parameter.value, Parameter.parameters, source, target);
                });
            }
        }
    }
}

public struct ActionList : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        for (int i = 0; i < parameters.Count; i++)
        {
            var Parameter = parameters[i];
            GameActionDataManager.instance.GameAction(Parameter.value, Parameter.parameters, source, target);
        }
    }
}

public struct DisplayHurt : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int targetId;
    public string hurtValue;
    public HurtResultType hurtResultType;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 3)
        {
            targetId = int.Parse(parameters[0].value);
            hurtValue = parameters[1].value;
            hurtResultType = (HurtResultType)int.Parse(parameters[2].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct NextActionSkillEstimate : GameAction
{
    public int skillId;
    public int sourceId;
    public bool displayHurt;
    public List<int> targets;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 5)
        {
            skillId = int.Parse(parameters[0].value);
            sourceId = int.Parse(parameters[1].value);
            displayHurt = bool.Parse(parameters[2].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct ActionSkillEstimate : GameAction
{
    public int skillId;
    public int sourceId;
    public int targetId;
    public int index;
    public bool displayHurt;

    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 5)
        {
            skillId = int.Parse(parameters[0].value);
            sourceId = int.Parse(parameters[1].value);
            targetId = int.Parse(parameters[2].value);
            index = int.Parse(parameters[3].value);
            displayHurt = bool.Parse(parameters[4].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct HideFightScene : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DisplayFightScene : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }

}

public struct JumpFilm : GameAction
{
    public string filmName;
    public float jumpTime;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 2)
        {
            filmName = parameters[0].value;
            jumpTime = float.Parse(parameters[1].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct SimpleTalk : GameAction
{
    public int talkId, characterId;
    public Action endAction;
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
            talkId = int.Parse(parameters[0].value);
        if (parameters.Count >= 2)
        {
            characterId = int.Parse(parameters[1].value);
        }
        if (target != int.MinValue)
        {
            talkId = target;
        }
        if (source != int.MinValue)
        {
            characterId = source;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DynamicTalk : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public string content;
    public Sprite icon;
    public string name;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct Talk : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int talkId, characterId;
    public bool displayFunction;
    public int nextTalkEventId;
    public List<int> fixedFunctions;
    public Action endAction;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        displayFunction = false;
        if (parameters.Count >= 1)
        {
            talkId = int.Parse(parameters[0].value);
            if (parameters.Count >= 2)
            {
                characterId = int.Parse(parameters[1].value);
            }
            else
            {
                characterId = -1;
            }
        }
        else
        {
            characterId = -1;
        }



        if (parameters.Count >= 3)
        {
            displayFunction = bool.Parse(parameters[2].value);
        }
        fixedFunctions = null;
        if (parameters.Count >= 4)
        {
            fixedFunctions = GameCommon.StringToListInt(parameters[3].value);
        }

        if (source != 0 && source != int.MinValue)
        {
            characterId = source;
        }
        if(target!=0&& target != int.MinValue)
        {
            talkId = target;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct LoadMapCompleted : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct StartWorldInit : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct SwitchScene : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public string sceneName;
    public int beforeLoadActionId, afterLoadActionId;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
            sceneName = parameters[0].value;
        if (parameters.Count >= 2)
            beforeLoadActionId = int.Parse(parameters[1].value);
        if (parameters.Count >= 3)
            afterLoadActionId = int.Parse(parameters[2].value);
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct ChapterStepAction : GameAction
{
    public SetResult setResult { set; get; }

    public SetValue setValue { get; set; }
    public void Clear() { this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct OpenChapter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int id;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            id = int.Parse(parameters[0].value);
        }
        if (source != 0 && source != int.MinValue)
        {
            id = source;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct EnterChapter : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public int id;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            id = int.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct DisplayFilm : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public string filmName;
    public string path;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            filmName = parameters[0].value;
        }
        if (parameters.Count > 1)
        {
            path = parameters[1].value;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct HideFilm : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public string filmName;
    public string path;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count > 0)
        {
            filmName = parameters[0].value;
        }
        if (parameters.Count > 1)
        {
            path = parameters[1].value;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct StopFilm : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public string filmName;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 0)
        {
            filmName = parameters[0].value;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct PlayFilm : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { get; set; }  public void Clear(){this = default; }
    public string filmName;
    public string assetName;

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            filmName = parameters[0].value;
        }
        if (parameters.Count >= 2)
        {
            assetName = parameters[1].value;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct PauseFilm : GameAction
{
    public string filmName;
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 0)
        {
            filmName = parameters[0].value;
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, true);
    }
}

public struct EndNowRoundFight : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
public struct TryStartAutoExplore : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct TryStartAutoBehavior : GameAction
{
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SwitchAutoExplore : GameAction
{
    public bool explore;
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }

    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        this.setResult = setResult;
        this.setValue = setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SetAutoExplore : GameAction
{
    public bool auto;
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            auto = bool.Parse(parameters[0].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}

public struct SwitchFunctionButton : GameAction
{
    public bool fight;
    public bool auto;
    public SetValue setValue { get; set; }
    public SetResult setResult { set; get; }
    public void Clear() { this = default; }
    public void Init(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
        if (parameters.Count >= 1)
        {
            fight = bool.Parse(parameters[0].value);
        }
        if (parameters.Count >= 2)
        {
            auto = bool.Parse(parameters[1].value);
        }
        this.setResult = setResult;
        this.setValue=setValue;
        GameActionManager.instance.QueueAction(this, immediately);
    }
}
