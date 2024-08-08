using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GameTimerController : Singleton<GameTimerController>
{
    private ConcurrentDictionary<Action, CancellationTokenSource> waitTasks = new ConcurrentDictionary<Action, CancellationTokenSource>();

    Queue<Action> activeActions = new Queue<Action>();
    public override bool NeedUpdata => true;

    public override void Init()
    {
        base.Init();
    }
    protected override void Clear()
    {
        base.Clear();
        waitTasks.Clear();
        activeActions.Clear();

    }

    public void RemoveWaiter(Action action)
    {
        if (action == null)
        {
            return;
        }
        try
        {
            if (waitTasks.TryRemove(action, out var tokenSource))
            {
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
        }
        catch
        { 
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
            activeActions.Enqueue(action);
            //action.Invoke();
            waitTasks.TryRemove(action, out var tokenSource);
           // waitTasks.Remove(action);
            tokenSource2.Dispose();
        }, tokenSource2.Token);
        waitTasks.TryAdd(action, tokenSource2);
        //waitTasks[action] = tokenSource2;
    }

    protected override void UpData()
    {
        base.UpData();
        while(activeActions.Count > 0)
        {
            var Action = activeActions.Dequeue();
            if (Action != null)
            {
                Action.Invoke();
            } 
        }
    }
}