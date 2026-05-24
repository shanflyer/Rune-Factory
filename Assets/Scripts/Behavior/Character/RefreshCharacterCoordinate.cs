using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("更新角色位置")]
public class RefreshCharacterCoordinate : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt3 target;

    Character character;

    int oldCharacterId;
    public override void OnStart()
    {
        if (oldCharacterId != characterId.Value)
        {
            oldCharacterId = characterId.Value;
            character = CharacterManager.instance.GetCharacter(characterId.Value);
        }
        if (character!=null)
        {
            if (character.ObjCoordinate.Equals(target.Value))
            {
                target.Value = character.ObjCoordinate;
                taskStatus = TaskStatus.Success;
            }
        }
        taskStatus = TaskStatus.Failure;

    }
    TaskStatus taskStatus;


    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}
