using System.Collections;
using UnityEngine; 
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

[TaskCategory("NewGame/回合/事件")]
[TaskName("检查战斗角色数量")]
public class CheckFightCharacterCount : Action
{
    public int checkCount;
    public CompareType compareType;
    ShardQueneInt fightCharacters; 
    public override void OnStart()
    {
        if (fightCharacters == null)
        {
            fightCharacters = (ShardQueneInt)Owner.GetVariable("fightCharacters");
        } 
    }

    public override TaskStatus OnUpdate()
    {
        switch (compareType)
        {
            case CompareType.等于:
                if (fightCharacters.Value.Count == checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.不等于:
                if (fightCharacters.Value.Count != checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.大于:
                if (fightCharacters.Value.Count > checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.不大于:
                if (fightCharacters.Value.Count !> checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.小于:
                if (fightCharacters.Value.Count < checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.不小于:
                if (fightCharacters.Value.Count !< checkCount)
                {
                    return TaskStatus.Success;
                }
                break;
        }
        return TaskStatus.Failure;
    }
}