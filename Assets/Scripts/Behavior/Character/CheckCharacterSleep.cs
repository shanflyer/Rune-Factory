using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/Character")]
[TaskName("检查角色是否睡觉")]
public class CheckCharacterSleep : Action
{
    public SharedInt characterId;
    private NPC npc;

    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull()) characterId = (SharedInt)Owner.GetVariable("CharacterId");
    }

    public override TaskStatus OnUpdate()
    {
        if (npc == null) NPCManager.instance.GetNPCFormInstance(characterId.Value, out npc);

        if (npc != null && npc.startSleepHour >= 0)
            return TaskStatus.Success;
        return TaskStatus.Failure;
    }
}