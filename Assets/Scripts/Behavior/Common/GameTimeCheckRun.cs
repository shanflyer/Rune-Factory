using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("NewGame/Common")]
[TaskName("时间满足条件")]
public class GameTimeCheckRun : Action
{
    public SharedInt year0;
    public SharedInt season0;
    public SharedInt day0;
    public SharedInt hour0;
    public SharedInt minute0;

    public SharedInt year1;
    public SharedInt season1;
    public SharedInt day1;
    public SharedInt hour1;
    public SharedInt minute1;
    public bool Equals = true;

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
    }

    private void UpdateGameTime(UpdateGameTime updateGameTime)
    {
        bool result = true;
        if (year0.Value > 0 && year1.Value >= year0.Value)
        {
            if (Equals)
            {
                if (updateGameTime.year < year0.Value || updateGameTime.year > year1.Value)
                {
                    result = false;
                }
            }
            else
            {
                if (updateGameTime.year >= year0.Value && updateGameTime.year <= year1.Value)
                {
                    result = false;
                }
            }
        }
        if (!result)
        {
            SetResult(false);
        }
        if (season0.Value > 0 && season1.Value >= season0.Value)
        {
            if (Equals)
            {
                if (updateGameTime.season < season0.Value || updateGameTime.season > season1.Value)
                {
                    result = false;
                }
            }
            else
            {
                if (updateGameTime.season >= season0.Value && updateGameTime.season <= season1.Value)
                {
                    result = false;
                }
            }
        }
        if (!result)
        {
            SetResult(false);
        }
        if (day0.Value > 0 && day1.Value >= day0.Value)
        {
            if (Equals)
            {
                if (updateGameTime.day < day0.Value || updateGameTime.day > day1.Value)
                {
                    result = false;
                }
            }
            else
            {
                if (updateGameTime.day >= day0.Value && updateGameTime.day <= day1.Value)
                {
                    result = false;
                }
            }
        }
        if (!result)
        {
            SetResult(false);
        }
        if (hour0.Value >= 0 && hour1.Value >= hour0.Value)
        {
            if (Equals)
            {
                if (updateGameTime.hour < hour0.Value || updateGameTime.hour > hour1.Value)
                {
                    result = false;
                }
            }
            else
            {
                if (updateGameTime.hour >= hour0.Value && updateGameTime.hour <= hour1.Value)
                {
                    result = false;
                }
            }
        }
        if (!result)
        {
            SetResult(false);
        }
        if (minute0.Value >= 0 && minute1.Value >= minute0.Value)
        {
            if (Equals)
            {
                if (updateGameTime.minute < minute0.Value || updateGameTime.minute > minute1.Value)
                {
                    result = false;
                }
            }
            else
            {
                if (updateGameTime.minute >= minute0.Value && updateGameTime.minute <= minute1.Value)
                {
                    result = false;
                }
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