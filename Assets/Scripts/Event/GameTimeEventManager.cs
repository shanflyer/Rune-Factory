using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;

public class GameTimeEventManager:Singleton<GameTimeEventManager>
{
    // public override bool NeedUpdata => true; 
    MyNativeData<GameTimeEvent> newDayTimeEvents;
    MyNativeData<GameTimeEvent> wakeUpTimeEvents;
    private int newDayActionIndex = 0;
    private int newWakeUpActionIndex = 0;
    public override async void Init()
    {
        base.Init(); 
        newDayTimeEvents.Init(16);
        wakeUpTimeEvents.Init(16);
        var gameTimeEventDatas =await GameDataManager.instance.GetAllAsyncData<GameTimeEventData>();
        for(int i=0;i<gameTimeEventDatas.Count;i++)
        {
            var data = gameTimeEventDatas[i];
            if (CheckGameTimeEventForSave(data.id))
            {
                GameTimeEvent gameTimeEvent = new GameTimeEvent
                { 
                    timeEventType = data.timeEventType,
                    id = data.id,
                    triggerValue = data.triggerValue,
                    actionValue = data.actionValue,
                };
                switch (data.timeEventType)
                {
                    case TimeEventType.时间序列:
                        newDayTimeEvents.SetData(gameTimeEvent);
                        break;
                    case TimeEventType.苏醒序列:
                        wakeUpTimeEvents.SetData(gameTimeEvent);
                        break;
                } 
               
            }
        }

        GameActionManager.instance.AddListener<NewDay>(CheckGameTimeEventNewDay);
        GameActionManager.instance.AddListener<PlayerWakeUp>(PlayerWakeUp);
    }
    protected override void Clear()
    {
        base.Clear();
        newDayTimeEvents.Dispose();
        wakeUpTimeEvents.Dispose();
    }
    void CheckGameTimeEventNewDay(NewDay NewDay)
    {
        newDayActionIndex++;
        List<int> deathEvents = new List<int>();
        foreach(GameTimeEvent gameTimeEvent in newDayTimeEvents)
        {
            if(gameTimeEvent.triggerValue== newDayActionIndex)
            {
                GameActionDataManager.instance.Action(gameTimeEvent.actionValue);
                deathEvents.Add(gameTimeEvent.id);
            }
        }
        for(int i = 0; i < deathEvents.Count; i++)
        {
            newDayTimeEvents.RemoveData(deathEvents[i]);
        }
    }

    void PlayerWakeUp(PlayerWakeUp PlayerWakeUp)
    {
        newWakeUpActionIndex++;
        List<int> deathEvents = new List<int>();
        foreach (GameTimeEvent gameTimeEvent in wakeUpTimeEvents)
        {
            if (gameTimeEvent.triggerValue == newWakeUpActionIndex)
            {
                GameActionDataManager.instance.Action(gameTimeEvent.actionValue);
                deathEvents.Add(gameTimeEvent.id);
            }
        }
        for (int i = 0; i < deathEvents.Count; i++)
        {
            wakeUpTimeEvents.RemoveData(deathEvents[i]);
        }
    }
    protected override void UpData()
    {
        base.UpData();
    }

    bool CheckGameTimeEventForSave(int id)
    {
        return true;
    }

    public struct GameTimeEvent : INativeData
    { 
        public int id;
        public TimeEventType timeEventType;
        public int triggerValue;
        public int actionValue;
        public int nowActionIndex;

        public int Key => id;

        public void Dispose()
        { 
        }
    }
    public enum GameTimeEventState
    {
        存活, 休眠, 死亡
    }
}
