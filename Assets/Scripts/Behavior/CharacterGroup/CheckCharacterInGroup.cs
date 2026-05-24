using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Game/CharacterGroup")]
[TaskName("检查角色是否在团体中")]
public class CheckCharacterInGroup : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt groupId;

    public override TaskStatus OnUpdate()
    {
        Character character = CharacterManager.instance.GetCharacter(characterId.Value);

        groupId.Value = character.groupId;
        if (character.groupId == 0)
        {
            return TaskStatus.Failure;
        }
        else
        {
            return TaskStatus.Success;
        }

    }
}
