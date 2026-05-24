using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/农场")]
[TaskName("尝试平整土地")]
public class TrySmoothFieldBeHavior : Action
{
    public SharedInt targetId;
    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        TrySmoothField trySmoothField = new TrySmoothField
        {
            fieldId=targetId.Value,
            setResult=CheckState
        };
        GameActionManager.instance.QueueAction(trySmoothField);
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
