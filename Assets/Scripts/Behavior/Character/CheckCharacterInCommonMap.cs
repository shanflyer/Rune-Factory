using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Game/Character")]
[TaskName("检查角色是否在同一个地图")]
public class CheckCharacterInCommonMap : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt targetId;

    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
    }

    public override TaskStatus OnUpdate()
    {
        Character character = CharacterManager.instance.GetCharacter(characterId.Value);
        Character targetNPC = NPCManager.instance.GetNPCCharacter(targetId.Value);
        if (character != null && targetNPC != null)
        {
            if(character.mapInstance==targetNPC.mapInstance)
            {
                return TaskStatus.Success;
            }
        }

        return TaskStatus.Failure;
    }
}