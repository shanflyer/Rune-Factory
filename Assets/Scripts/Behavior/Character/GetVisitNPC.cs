using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;


[TaskCategory("Game/Character")]
[TaskName("获取要拜访的对象")]
public class GetVisitNPC : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField] private SharedInt visitNPC; 
    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (NPCManager.instance.GetNPCFormInstance(characterId.Value, out var npc))
        {
            int visitNpc = npc.GetVisitFriend();
            this.visitNPC.SetValue(visitNpc);
            if (visitNpc > 0)
            { 
                return TaskStatus.Success;
            }
        }

        return TaskStatus.Failure;
    }
}