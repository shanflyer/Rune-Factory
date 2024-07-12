using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NewGame/Common")]
[TaskName("时间流逝满足条件")]
public class GameTimePassCheck : Action
{  
    public SharedInt year;
    public SharedInt season;
    public SharedInt day;
    public SharedInt hour;
    public SharedInt minute; 


    private int oldYear,oldSeason,oldDay,oldHour,oldMinute;
    public override void OnAwake()
    {
        base.OnAwake();
    }

    private bool addAction = false;

    public override void OnBehaviorComplete()
    {
        base.OnBehaviorComplete();
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<UpdateGameTime>(UpdateGameTime);
        addAction = false;
    }

    public override void OnEnd()
    {
        base.OnEnd();
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<UpdateGameTime>(UpdateGameTime);
        addAction = false;
    }

    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        if (!addAction)
        {
            GameActionManager.instance.AddListener<UpdateGameTime>(UpdateGameTime);
            addAction = true;
        }

        oldYear = GameTimeManager.instance.Year;
        oldDay = GameTimeManager.instance.Day;
        oldHour = GameTimeManager.instance.Hour;
        oldMinute = GameTimeManager.instance.Minute;
        oldSeason = (int)GameTimeManager.instance.Season;
    }

    private void UpdateGameTime(UpdateGameTime updateGameTime)
    {
        bool result = true;
        int _year = year.Value > 0 ? year.Value : oldYear;
        if (updateGameTime.year > _year)
        {
            SetResult(true);
            return;
        }
        if (updateGameTime.year < _year)
        {
            result = false;
        }

        if (result)
        {
            int _season = season.Value > 0 ? season.Value : oldSeason;
            if (updateGameTime.season > _season)
            {
                SetResult(true);
                return;
            }
            if (updateGameTime.season < _season)
            {
                result = false;
            }
        }
        if (result)
        {
            int _day = day.Value > 0 ? day.Value : oldDay;
            if (updateGameTime.day > _day)
            {
                SetResult(true);
                return;
            }
            if (updateGameTime.day < _day)
            {
                result = false;
            }
        }
        if (result)
        {
            int _hour = hour.Value > 0 ? hour.Value : oldHour;
            if (updateGameTime.hour > _hour)
            {
                SetResult(true);
                return;
            }
            if (updateGameTime.hour < _hour)
            {
                result = false;
            }
        }
        if (result)
        {
            int _minute = minute.Value > 0 ? minute.Value : oldMinute;
            if (updateGameTime.minute > _minute)
            {
                SetResult(true);
                return;
            }
            if (updateGameTime.minute < _minute)
            {
                result = false;
            }
        }

        if (!result)
        {
            SetResult(false);
        }
        else
        {
            SetResult(true);
        }
    }

    private void SetResult(bool result)
    {
        if (result)
        {
            taskStatus = TaskStatus.Success;
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }
    }

    private TaskStatus taskStatus;

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}