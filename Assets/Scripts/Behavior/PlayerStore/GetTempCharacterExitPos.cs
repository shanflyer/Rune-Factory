using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics; 

[TaskCategory("Game/PlayerStore")]
[TaskName("获取消费者退出位置")]
public class GetTempCharacterExitPos : Action
{
    private SharedInt characterId;
    public SharedInt3 targetCoordinate;
    public override void OnStart()
    {
        if (characterId == null)
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (targetCoordinate == null)
        {
            targetCoordinate = (SharedInt3)Owner.GetVariable("TargetCoordinate");
            if (targetCoordinate == null)
            {
                targetCoordinate = new SharedInt3();
                Owner.SetVariable("TargetCoordinate", targetCoordinate);
            }
        }
        taskStatus = TaskStatus.Running;

        GetTempCharacterExit getTempCharacterExit = new GetTempCharacterExit
        {
            SetInt3Value =
            (int3 value) =>
            {
                targetCoordinate.SetValue(value);
                taskStatus = TaskStatus.Success;
            }
        };
        GameActionManager.instance.QueueAction(getTempCharacterExit);
    }

    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}