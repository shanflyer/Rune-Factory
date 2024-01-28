using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Game/Character")]
[TaskName("检查角色是否在队伍")]
public class CharacterIsTeamer : Action
{
    public SharedInt characterId;
    public Character character;
    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
            
        }
        character = CharacterManager.instance.GetCharacter(characterId.Value);
    }

    public override TaskStatus OnUpdate()
    {
        if (character.isInTeam)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}