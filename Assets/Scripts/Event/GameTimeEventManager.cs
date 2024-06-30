using System.Collections.Generic;

public class GameTimeEventManager : Singleton<GameTimeEventManager>
{
    // public override bool NeedUpdata => true;
    private Dictionary<int,GameTimeEvent> newDayTimeEvents=new Dictionary<int, GameTimeEvent>();

    private Dictionary<int, GameTimeEvent> wakeUpTimeEvents=new Dictionary<int, GameTimeEvent>();
    private int newDayActionIndex = 0;
    private int newWakeUpActionIndex = 0;

    public override async void Init()
    {
        base.Init();
        newDayTimeEvents.Clear();
        wakeUpTimeEvents.Clear();
        var gameTimeEventDatas = await GameDataManager.instance.GetAllAsyncData<GameTimeEventData>();
        for (int i = 0; i < gameTimeEventDatas.Count; i++)
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
                        newDayTimeEvents.Add(gameTimeEvent.Key,gameTimeEvent);
                        break;

                    case TimeEventType.苏醒序列:
                        wakeUpTimeEvents.Add(gameTimeEvent.Key, gameTimeEvent);
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
        newDayTimeEvents.Clear();
        wakeUpTimeEvents.Clear();
    }

    private void CheckGameTimeEventNewDay(NewDay NewDay)
    {
        newDayActionIndex++;
        List<int> deathEvents = new List<int>();
        foreach (var gameTimeEvent in newDayTimeEvents.Values)
        {
            if (gameTimeEvent.triggerValue == newDayActionIndex)
            {
                GameActionDataManager.instance.Action(gameTimeEvent.actionValue);
                deathEvents.Add(gameTimeEvent.id);
            }
        }
        for (int i = 0; i < deathEvents.Count; i++)
        {
            newDayTimeEvents.Remove(deathEvents[i]);
        }
    }

    private void PlayerWakeUp(PlayerWakeUp PlayerWakeUp)
    {
        newWakeUpActionIndex++;
        List<int> deathEvents = new List<int>();
        foreach (var  gameTimeEvent in wakeUpTimeEvents.Values)
        {
            if (gameTimeEvent.triggerValue == newWakeUpActionIndex)
            {
                GameActionDataManager.instance.Action(gameTimeEvent.actionValue);
                deathEvents.Add(gameTimeEvent.id);
            }
        }
        for (int i = 0; i < deathEvents.Count; i++)
        {
            wakeUpTimeEvents.Remove(deathEvents[i]);
        }
    }

    protected override void UpData()
    {
        base.UpData();
    }

    private bool CheckGameTimeEventForSave(int id)
    {
        return true;
    }

    public class GameTimeEvent 
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