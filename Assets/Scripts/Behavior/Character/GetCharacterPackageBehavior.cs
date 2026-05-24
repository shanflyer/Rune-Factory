using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Game/Character")]
[TaskName("获取角色背包")]
public class GetCharacterPackageBehavior : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt packageId;
    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = Owner.GetVariable("CharacterId") as SharedInt;
        }
        if (characterId != null)
        {
            Character character = CharacterManager.instance.GetCharacter(characterId.Value);
            packageId.Value = character.characterPackage;
            taskStatus = TaskStatus.Success;
            return;
        }
        taskStatus = TaskStatus.Failure;
    }

    TaskStatus taskStatus;

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}
