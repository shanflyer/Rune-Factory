using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;

public interface GameEvent
{
    public int GetEventId();
    public bool IsAwake();
    public bool IsDead();
    public bool CheckTrigger();
    public void EventAction();
    public void SetEventDead();

    public void StopTimer();
    public void SetAwake(bool awake);
    public void SetTrigger(bool trigger);
    public void SetDead(bool dead);
}

public struct WaitTimeEvent: GameEvent
{
    private int id;
    private Timer timer;
   // private float waitTime;
    private bool trigger;
    private bool awake;
    private bool dead;
    public WaitTimeEvent(int id, float waitTime)
    {
        this.id = id;
        trigger = false;
        awake = false;
        dead = false;

        //this.waitTime = waitTime;
        timer = new Timer();
        timer.Elapsed += new ElapsedEventHandler(WaitEndAction);
        timer.Interval = waitTime;
        timer.AutoReset = false;
        timer.Enabled = false;
    }
    public void SetAwake(bool awake)
    {
        this.awake = awake;
        timer.Enabled = true;
    }
    public bool CheckTrigger()
    {
        return trigger;
    }

    public void EventAction()
    {
        //throw new System.NotImplementedException();
    }

    public int GetEventId()
    {
        return id;
    }

    public bool IsAwake()
    {
        return awake;
    }

    public bool IsDead()
    {
        return dead;
    }

    public void SetEventDead()
    {
        dead = true ;
    }

    public void StopTimer()
    {
        timer.Stop();
        timer.Dispose();
    }

    void WaitEndAction(object sender,ElapsedEventArgs e)
    {
        trigger = true;
    }

    public void SetTrigger(bool trigger)
    {
        this.trigger = trigger;
    }

    public void SetDead(bool dead)
    {
        this.dead = dead;
    }
}
