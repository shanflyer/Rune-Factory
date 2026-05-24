using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/Character")]
[TaskName("解除角色关联")]
public class CharacterUnLink : Action
{
    public SharedInt characterId;

    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull()) characterId = (SharedInt)Owner.GetVariable("CharacterId");
    }

    public override TaskStatus OnUpdate()
    {
        var character = CharacterManager.instance.GetCharacter(characterId.Value);
        if (character != null) character.UnLinkItem();

        return TaskStatus.Success;
    }
}
