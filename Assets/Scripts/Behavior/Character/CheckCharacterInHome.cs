using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Game/Character")]
[TaskName("检查角色是不是在家")]
public class CheckCharacterInHome : Action
{
    [SerializeField]
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
        if(NPCManager.instance.GetNPCFormInstance(characterId.Value,out var npc))
        {
            if (npc.IsInHome())
            {
                return TaskStatus.Success;
            }
        }
        return TaskStatus.Failure;
    }
}
