using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/农场")]
[TaskName("尝试浇水")]
public class TrySetWaterFieldBeHavior : Action
{
    public SharedInt targetId;
    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        SetWaterField setWaterField = new SetWaterField
        {
            fieldId = targetId.Value,
            setResult = CheckState
        };
        GameActionManager.instance.QueueAction(setWaterField);
    }
    void CheckState(bool value)
    {
        if (value)
        {
            taskStatus = TaskStatus.Success;
            return;
        }
        taskStatus = TaskStatus.Failure;
    }
    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}