using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame")]
[TaskName("获取可战斗角色数量")]
public class GetFightCharacterCountBehavior : Action
{
    [SerializeField]
    SharedInt fighterCount;

    public override void OnStart()
    {
        fighterCount.Value = FightManager.instance.GetActiveFightCharacterCount();
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
