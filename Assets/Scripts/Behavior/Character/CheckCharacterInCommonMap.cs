using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

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
        var character = CharacterManager.instance.GetCharacter(characterId.Value);
        if (character != null && NPCManager.instance.GetNPCFormInstance(targetId.Value, out var targetNPC))
        {
            if (character.mapInstance == targetNPC.Character.mapInstance)
            {
                return TaskStatus.Success;
            }
        }

        return TaskStatus.Failure;
    }
}
