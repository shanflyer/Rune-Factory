using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("角色暂停行为")]
public class NPCPauseBehavior : Action
{
    [SerializeField]
    private SharedInt characterId; 

    Character character;
    TaskStatus taskStatus;
    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        
        character = CharacterManager.instance.GetCharacter(characterId.Value);
        if (character != null)
        {
            character.StopMove();
            NPCTaskScheduleManager.instance.SetOverrideHold(character.instanceId, true);
            NPCTaskScheduleManager.instance.PauseCharacterBehavior(character.instanceId);
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