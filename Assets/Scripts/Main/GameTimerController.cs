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
    private Dictionary<Action,IEnumerator> waitIenumerators= new Dictionary<Action,IEnumerator>();


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
        if (waitIenumerators.TryGetValue(action, out var enumerator))
        {
            GameController.instance.StopCoroutine(enumerator);
        }
        return;

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
        if(waitIenumerators.TryGetValue(action,out var enumerator))
        {
            GameController.instance.StopCoroutine(enumerator);
        }
        enumerator = WaitAction(delay, action);
        GameController.instance.StartCoroutine(enumerator);
        waitIenumerators[action] = enumerator;
        return;

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
           // Debug.Log($"延时入队{action.Target}");
            //action.Invoke();
            waitTasks.TryRemove(action, out var tokenSource);
           // waitTasks.Remove(action);
            tokenSource2.Dispose();
        }, tokenSource2.Token);
        waitTasks.TryAdd(action, tokenSource2); 
    }

    IEnumerator WaitAction(int delay, Action action)
    {
        float waitTime = delay * 0.001f;
        float timeValue = 0; 
        while (timeValue<waitTime)
        {
            timeValue += Time.deltaTime;
            
            yield return 0;
        }
        action.Invoke();
        Debug.Log($"target:{action.Target}-Method:{action.Method}");
       
    }

    protected override void UpData()
    {
        base.UpData();
        while(activeActions.Count > 0)
        { 
            var Action = activeActions.Dequeue();
          //  Debug.Log($"延时出队{Action.Target}");
            if (Action != null)
            {
                Action.Invoke();
            } 
        }
    }
}