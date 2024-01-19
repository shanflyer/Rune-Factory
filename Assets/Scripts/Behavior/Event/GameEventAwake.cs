using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("NewGame/Event")]
[TaskName("事件苏醒")]
public class GameEventAwake : Action
{
    public SharedInt gameEventId;

    [Header("睡眠、苏醒")]
    public bool awake;

    [Header("睡眠时暂停或重置")]
    public bool pause;

    // Start is called before the first frame update
    public override void OnStart()
    {
        GameEventManager.instance.SetGameEventAwake(gameEventId.Value, awake, pause);
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}