
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
    public void DeleyActionMain(int delay, Action action)
    {
        GameController.instance.StartCoroutine(Wait());
        IEnumerator Wait()
        {
            yield return new WaitForSeconds(delay*0.001f);
            action.Invoke();
        }
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