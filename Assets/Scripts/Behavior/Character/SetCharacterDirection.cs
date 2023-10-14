using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime; 
using System.Collections.Generic;
using System.Linq;   

[TaskCategory("Game/Character")]
[TaskName("设置角色面向目标")]
public class SetCharacterDirection : Action
{
    public SharedInt2 faceTargetCoordinate;
    private SharedInt characterId;
    public override void OnStart()
    {
        if (characterId == null)
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (faceTargetCoordinate == null)
        {
            faceTargetCoordinate = (SharedInt2)Owner.GetVariable("FaceTargetCoordinate");
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (characterId == null || faceTargetCoordinate == null)
        {
            return TaskStatus.Failure;
        }
        else
        {
            SetTargetDirection SetTargetDirection = new SetTargetDirection
            {
                characterId = characterId.Value,
                targetCoordinate = faceTargetCoordinate.Value
            };
            GameActionManager.instance.QueueAction(SetTargetDirection);
        }

        return TaskStatus.Success;
    }
}