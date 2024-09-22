using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

[TaskCategory("NewGame/回合/事件")]
[TaskName("检查当前战斗角色是否完成战斗")]
public class CheckNowFightCharacterActionEnd : Action
{
    [SerializeField]
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
            Debug.Log($"{fightCharacter.instanceId}--{fightCharacter.fightStatus}");
            if (fightCharacter.fightStatus == FightStatus.准备||fightCharacter.fightCharacterStaues!=FightCharacterStaues.正常)
            {
                return TaskStatus.Success;
            }
            return TaskStatus.Failure;
        }
        else
        {
            return TaskStatus.Success;
        } 
    }
}