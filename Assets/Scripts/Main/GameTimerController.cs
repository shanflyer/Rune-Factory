
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GameTimerController : Singleton<GameTimerController>
{
    Dictionary<Delegate, IEnumerator> waitIEnumerators = new Dictionary<Delegate, IEnumerator>();
    Dictionary<Delegate, CancellationTokenSource> waitTasks = new Dictionary<Delegate, CancellationTokenSource>();
    public override bool NeedUpdata => true;
    public override void Init()
    {
        base.Init();
    }
    public void DeleyActionMain(int delay, Action action)
    {
        if (waitIEnumerators.TryGetValue(action,out var ienumerator))
        {
            GameController.instance.StopCoroutine(ienumerator);
        }
        IEnumerator IEnumerator = Wait();
        waitIEnumerators[action] = ienumerator;
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(delay*0.001f);
            action.Invoke();
            waitIEnumerators.Remove(action);
        }
        GameController.instance.StartCoroutine(IEnumerator);
    }
    public void DelayAction(int delay,Action action)
    {
        if (waitTasks.TryGetValue(action, out var tokenSource))
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
            waitTasks.Remove(action);
            tokenSource2.Dispose();
        }, tokenSource2.Token);
        waitTasks[action] = tokenSource2;
    }
}