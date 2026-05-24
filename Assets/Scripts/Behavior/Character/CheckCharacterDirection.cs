using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("检查角色方向")]
public class CheckCharacterDirection : Action
{
    public Direction target;
    private SharedInt characterId;
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
        if (character != null && character.direction == target)
        {
           // Debug.Log("target:" + target);
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}
