using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEditor.Localization.Plugins.XLIFF.V20;


[TaskCategory("Game/Character")]
[TaskName("检查NPC行为状态")]
public class CheckNPCBehaviorState : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private NPCBehaviorState nPCBehaviorState;
    [SerializeField]
    private bool isEqual = true;
    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
    }

    public override TaskStatus OnUpdate()
    {
        var state =NPCTaskScheduleManager.instance.GetNPCBehaviorState(characterId.Value);
        if (state == NPCBehaviorState.NULL)
            return TaskStatus.Failure;
        if (isEqual)
        {
            return state == nPCBehaviorState ? TaskStatus.Success : TaskStatus.Failure;
        }
        else
        {
            return state != nPCBehaviorState ? TaskStatus.Success : TaskStatus.Failure;
        }
         
    }
}