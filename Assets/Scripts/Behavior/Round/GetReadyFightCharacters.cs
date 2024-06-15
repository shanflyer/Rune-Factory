using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/回合")]
[TaskName("获取已经ready的战斗对象")]
public class GetReadyFightCharacters : Action
{
    [SerializeField]
    SharedQueneInt fightCharacters;
    public override void OnStart()
    {
        if (fightCharacters == null)
        {
            fightCharacters = (SharedQueneInt)Owner.GetVariable("fightCharacters");
        }
        fightCharacters.Value = FightManager.instance.GetReadyFighter();
    }

    public override TaskStatus OnUpdate()
    {
       return TaskStatus.Success;
    }
}