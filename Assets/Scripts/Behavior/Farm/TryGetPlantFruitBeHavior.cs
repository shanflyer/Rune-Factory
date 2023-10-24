using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/农场")]
[TaskName("尝试采摘")]
public class TryGetPlantFruitBeHavior : Action
{
    public SharedInt fieldId; 

    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        TryGetPlantFruit tryGetPlantFruit = new TryGetPlantFruit
        {
            fieldId = fieldId.Value, 
            setResult = CheckState
        };
        GameActionManager.instance.QueueAction(tryGetPlantFruit);
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