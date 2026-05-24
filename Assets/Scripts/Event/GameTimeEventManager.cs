using System.Collections.Generic;

public class GameTimeEventManager : Singleton<GameTimeEventManager>
{
    // public override bool NeedUpdata => true;
    private Dictionary<int,GameTimeEvent> newDayTimeEvents=new Dictionary<int, GameTimeEvent>();

    private Dictionary<int, GameTimeEvent> wakeUpTimeEvents=new Dictionary<int, GameTimeEvent>();
    private System.Threading.Tasks.Task initializationTask = System.Threading.Tasks.Task.CompletedTask;
    public override System.Threading.Tasks.Task InitializationTask => initializationTask;
    private int newDayActionIndex
    {
        get => GameDataSaveManager.instance.UserGameSaveData.otherSaveData.newDayActionIndex;
        set
        {
            GameDataSaveManager.instance.UserGameSaveData.otherSaveData.newDayActionIndex = value;
        }
    }
    private int newWakeUpActionIndex
    {
        get => GameDataSaveManager.instance.UserGameSaveData.otherSaveData.newWakeUpActionIndex;
        set
        {
            GameDataSaveManager.instance.UserGameSaveData.otherSaveData.newWakeUpActionIndex = value;
        }
    }

    public override void Init()
    {
        base.Init();
        initializationTask = InitAsync();
    }

    private async System.Threading.Tasks.Task InitAsync()
    {
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

        GameActionManager.instance.AddAsyncListener<NewDay>(CheckGameTimeEventNewDayAsync, nameof(CheckGameTimeEventNewDayAsync));
        GameActionManager.instance.AddAsyncListener<PlayerWakeUp>(PlayerWakeUpAsync, nameof(PlayerWakeUp));
    }

    protected override void Clear()
    {
        base.Clear();
        initializationTask = System.Threading.Tasks.Task.CompletedTask;
        newDayTimeEvents.Clear();
        wakeUpTimeEvents.Clear();
    }

    private async System.Threading.Tasks.Task CheckGameTimeEventNewDayAsync(NewDay NewDay)
    {
        newDayActionIndex++;
        List<int> deathEvents = new List<int>();
        foreach (var gameTimeEvent in newDayTimeEvents.Values)
        {
            if (gameTimeEvent.triggerValue == newDayActionIndex)
            {
                await GameEventManager.instance.AddGameEvent(gameTimeEvent.actionValue);
                deathEvents.Add(gameTimeEvent.id);
                gameTimeEvent.nowActionIndex = newDayActionIndex;
            }
        }
        for (int i = 0; i < deathEvents.Count; i++)
        {
            newDayTimeEvents.Remove(deathEvents[i]);
        }
    }

    private async System.Threading.Tasks.Task PlayerWakeUpAsync(PlayerWakeUp PlayerWakeUp)
    {
        newWakeUpActionIndex++;
        List<int> deathEvents = new List<int>();
        foreach (var  gameTimeEvent in wakeUpTimeEvents.Values)
        {
            if (gameTimeEvent.triggerValue == newWakeUpActionIndex)
            { 
               await GameEventManager.instance.AddGameEvent(gameTimeEvent.actionValue);
                gameTimeEvent.nowActionIndex = newWakeUpActionIndex;
                deathEvents.Add(gameTimeEvent.id);
            }
        }
        for (int i = 0; i < deathEvents.Count; i++)
        {
            wakeUpTimeEvents.Remove(deathEvents[i]);
        }
    }

    protected override void Update()
    {
        base.Update();
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
