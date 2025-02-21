using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GameTimerController : Singleton<GameTimerController>
{ 
    private ConcurrentDictionary<Action,IEnumerator> waitEnumerators= new ConcurrentDictionary<Action,IEnumerator>();


    Queue<Action> activeActions = new Queue<Action>();
    public override bool NeedUpdata => true;

    public override void Init()
    {
        base.Init();
    }
    protected override void Clear()
    {
        base.Clear(); 
        activeActions.Clear();

    }

    public void RemoveWaiter(Action action)
    {
        if (action == null)
        {
            return;
        } 
        if (waitEnumerators.TryRemove(action, out var enumerator))
        {
            GameController.instance.StopCoroutine(enumerator);
        } 
        return;
 
      
        
    }
    public void DelayAction(int delay, Action action)
    {
        if (GameDataManager.instance.GlobalData.debug)
            Debug.Log($"target:{action.Target}-Method:{action.Method}");
        if (waitEnumerators.TryRemove(action,out var enumerator))
        {
            GameController.instance.StopCoroutine(enumerator); 
        }
        enumerator = WaitAction(delay, action);
        GameController.instance.StartCoroutine(enumerator); 
        waitEnumerators[action] = enumerator;
        return;

         
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
        if(waitEnumerators.TryRemove(action, out var enumerator))
        {
            GameController.instance.StopCoroutine(enumerator);
        }
        
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