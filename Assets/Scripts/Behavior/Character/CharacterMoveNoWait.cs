using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("角色移动指令")]
public class CharacterMoveNoWait : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt3 target;  
      
    Character character;
    TaskStatus taskStatus;
    public override void OnStart()
    { 
        taskStatus = TaskStatus.Running;
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (target == null || target.IsNull())
        {
            target = (SharedInt3)Owner.GetVariable("TargetCoordinate");
            if (target == null || target.IsNull())
            {
                taskStatus = TaskStatus.Failure;
                return;
            }
        }  
        var targetCoordinate = target.Value;
        character = CharacterManager.instance.GetCharacter(characterId.Value);
        if (character != null)
        {
            character.StopMove();
            if (!character.MoveCrossMap(targetCoordinate.z, targetCoordinate.xy, null))
            {
              
            }
            taskStatus = TaskStatus.Success;
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }
    }
     
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}