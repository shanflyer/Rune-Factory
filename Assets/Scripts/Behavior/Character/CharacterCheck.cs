using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Game/Character")]
[TaskName("检查角色")]
public class CharacterCheck : Action
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
        if(CharacterManager.instance.IsTempCharacter(characterId.Value))
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}