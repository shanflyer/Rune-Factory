using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Game/Character")]
[TaskName("检查Npc当前计划是否坚守位置")]
public class CheckNPCNowTaskCanPauseByMulit : Action
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
        if (NPCManager.instance.GetNPCFormInstance(characterId.Value,out var npc))
        {
            if (npc.holdPos)
            {
                return TaskStatus.Success;
            } 
        }
        return TaskStatus.Failure;
    }
}