using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

[TaskCategory("NewGame/回合/事件")]
[TaskName("检查当前战斗角色是否完成战斗")]
public class CheckNowFightCharacterActionEnd : Action
{
    SharedInt nowFightCharacter;
    public override void OnStart()
    {
        if (nowFightCharacter == null)
        {
            nowFightCharacter = (SharedInt)Owner.GetVariable("nowFightCharacter");
        }
    }

    public override TaskStatus OnUpdate()
    {
        FightCharacter fightCharacter;
        if(FightManager.instance.GetFightCharacter(nowFightCharacter.Value, out fightCharacter))
        {
            if (fightCharacter.fightStatus == FightStatus.攻击结束)
            {
                return TaskStatus.Success;
            }
        }
        
        return TaskStatus.Failure;
    }
}