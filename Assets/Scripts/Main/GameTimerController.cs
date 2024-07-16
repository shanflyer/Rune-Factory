using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GameTimerController : Singleton<GameTimerController>
{
    private Dictionary<Delegate, IEnumerator> waitIEnumerators = new Dictionary<Delegate, IEnumerator>();
    private ConcurrentDictionary<Delegate, CancellationTokenSource> waitTasks = new ConcurrentDictionary<Delegate, CancellationTokenSource>();
    public override bool NeedUpdata => true;

    public override void Init()
    {
        base.Init();
    }
    public void StopWaitDeleyAction( Action action)
    {
        if (waitIEnumerators.TryGetValue(action, out var ienumerator))
        {
            GameObjectCurveController.instance.UpDataComponent.StopCoroutine(ienumerator);
        }
        waitIEnumerators.Remove(action);
    }
    public void DeleyActionMain(int delay, Action action)
    {
        if (waitIEnumerators.TryGetValue(action, out var ienumerator))
        {
            GameObjectCurveController.instance.UpDataComponent.StopCoroutine(ienumerator);
        }
        ienumerator = Wait();
        waitIEnumerators[action] = ienumerator;
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(delay * 0.001f);
            action.Invoke();
            waitIEnumerators.Remove(action);
        }
        GameObjectCurveController.instance.UpDataComponent.StartCoroutine(ienumerator);
    }

    public void RemoveWaiter(Delegate @delegate)
    {
        if(waitTasks.TryRemove(@delegate,out var tokenSource))
        {
            tokenSource.Cancel();
            tokenSource.Dispose(); 
        }
        
    }
    public void DelayAction(int delay, Action action)
    {
        if (waitTasks.TryRemove(action, out var tokenSource))
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
        }
        var tokenSource2 = new CancellationTokenSource();
        CancellationToken ct = tokenSource2.Token;
        Task task = Task.Factory.StartNew(async () =>
        {
            await Task.Delay(delay);
            action.Invoke();
            waitTasks.TryRemove(action, out var tokenSource);
           // waitTasks.Remove(action);
            tokenSource2.Dispose();
        }, tokenSource2.Token);
        waitTasks.TryAdd(action, tokenSource2);
        //waitTasks[action] = tokenSource2;
    }
}