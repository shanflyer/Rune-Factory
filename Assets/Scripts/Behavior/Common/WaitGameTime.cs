using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("NewGame/Common")]
[TaskName("等待游戏世界时间")]
[TaskIcon("{SkinColor}WaitIcon.png")]
public class WaitGameTime : Action
{
    public SharedInt waitMinMinute;
    public SharedInt waitMaxMinute;

    private bool addAction = false;
    private bool zero = false;
    private int year, season, day, hour, minute;

    [SerializeField]
    private int waitMinute;

    [SerializeField]
    private int total;

    private void UpdateGameTime(UpdateGameTime updateGameTime)
    {
        if (zero)
        {
            year = updateGameTime.year;
            season = updateGameTime.season;
            day = updateGameTime.day;
            hour = updateGameTime.hour;
            minute = updateGameTime.minute;
            zero = false;
        }
        else
        {
            var _year = updateGameTime.year - year;
            var _season = updateGameTime.season - season;
            var _day = updateGameTime.day - day;
            var _hour = updateGameTime.hour - hour;
            var _minute = updateGameTime.minute - minute;
            total = (((_year * 4 + _season) * 30 + _day) * 24 + _hour) * 60 + _minute;
            if (total >= waitMinute)
            {
                taskStatus = TaskStatus.Success;
            }
        }
    }

    public override void OnBehaviorComplete()
    {
        base.OnBehaviorComplete();
        GameActionManager.instance.RemoveListener<UpdateGameTime>(UpdateGameTime);
        addAction = false;
    }

    public override void OnEnd()
    {
        base.OnEnd();
        GameActionManager.instance.RemoveListener<UpdateGameTime>(UpdateGameTime);
        addAction = false;
    }

    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        zero = true;
        waitMinute = GameRandom.RandomInt(waitMinMinute.Value, waitMaxMinute.Value);
        if (!addAction)
        {
            GameActionManager.instance.AddListener<UpdateGameTime>(UpdateGameTime);
            addAction = true;
        }
    }

    private TaskStatus taskStatus;

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}