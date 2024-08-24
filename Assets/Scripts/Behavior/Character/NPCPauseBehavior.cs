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
            if(NPCManager.instance.GetNPCFormInstance(character.instanceId,out var npc))
            {
                npc.SetOverrideHold(true);
                npc.PauseCharacterBehavior();
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