
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class GameTimerController : Singleton<GameTimerController>
{

    public override void Init()
    {
        base.Init();
    }
    public void DelayAction(int delay,Action action)
    {
        Task task = Task.Factory.StartNew(async () =>
        {
            await Task.Delay(delay);
            action.Invoke();
        });
    }
}