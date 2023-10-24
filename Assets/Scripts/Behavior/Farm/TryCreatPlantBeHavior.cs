using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/农场")]
[TaskName("尝试种植")]
public class TryCreatPlantBeHavior : Action 
{
    public SharedInt fieldId;
    public SharedInt plantId;

    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        TryCreatPlant tryCreatPlant = new TryCreatPlant
        {
            fieldId = fieldId.Value,
            plantId = plantId.Value,
            setResult = CheckState
        };
        GameActionManager.instance.QueueAction(tryCreatPlant);
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