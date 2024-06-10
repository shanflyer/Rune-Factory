using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/回合/事件")]
[TaskName("检查自身是否存活")]
public class CheckSelfSurvival : Action
{ 
    [SerializeField]
    SharedInt fightCharacter; 
    public override void OnStart()
    {
        if (fightCharacter == null)
            fightCharacter = (SharedInt)Owner.GetVariable("fightCharacter");
    }

    public override TaskStatus OnUpdate()
    {
        if (FightManager.instance.IsSurvival(fightCharacter.Value))
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}