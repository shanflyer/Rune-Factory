using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame")]
[TaskName("战斗角色完成行为")]
public class FightCharacterEndBehavior : Action
{
   
    public override void OnStart()
    {
        FightManager.instance.EndBehavior();
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}