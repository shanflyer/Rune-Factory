using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/回合/事件")]
[TaskName("获取敌我角色数量")]
public class GetMyOrEnemyFightCharacterCount : Action
{
    public int checkCount;
    public CompareType compareType;
    [SerializeField]
    SharedInt fighterCount;
    [SerializeField]
    SharedInt fightCharacter;
    [SerializeField]
    Force force;
    public override void OnStart()
    {
        if (fightCharacter == null)
            fightCharacter = (SharedInt)Owner.GetVariable("fightCharacter");
        fighterCount.Value = FightManager.instance.GetActiveFightCharacterCount(fightCharacter.Value, force);
    }

    public override TaskStatus OnUpdate()
    {
        switch (compareType)
        {
            case CompareType.等于:
                if (fighterCount.Value == checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.不等于:
                if (fighterCount.Value != checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.大于:
                if (fighterCount.Value > checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.不大于:
                if (fighterCount.Value! > checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.小于:
                if (fighterCount.Value < checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.不小于:
                if (fighterCount.Value! < checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
        }
        return TaskStatus.Failure;
    }
}