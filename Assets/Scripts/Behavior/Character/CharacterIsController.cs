using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Game/Character")]
[TaskName("检查角色是否控制角色")]
public class CharacterIsController : Action
{
    public SharedInt characterId;
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
        if (character != null && character.isController)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}
