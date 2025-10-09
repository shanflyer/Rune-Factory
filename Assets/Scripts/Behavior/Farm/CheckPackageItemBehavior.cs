using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/农场")]
[TaskName("检查土地状态")]
public class CheckPackageItemBehavior : Action
{ 
    public SharedInt targetId;
    public FieldState fieldState;
    public bool plantDeath;
    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        CheckFieldState checkFieldState = new CheckFieldState
        {
            instanceid = targetId.Value,
            plantDeath = plantDeath,
            setValue = CheckState
        };
        GameActionManager.instance.QueueAction(checkFieldState);
    }
    void CheckState(int value)
    {
        if (value == (int)fieldState)
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