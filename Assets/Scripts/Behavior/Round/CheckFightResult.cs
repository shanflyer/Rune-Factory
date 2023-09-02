using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using static BehaviorDesigner.Runtime.BehaviorManager;
using Unity.Mathematics;

[TaskCategory("NewGame/回合/事件")]
[TaskName("检查战斗是否结束")]
public class CheckFightResult : Action
{
	SharedBool FightResult;
	public override void OnStart()
	{
        if (FightResult == null)
            FightResult = Owner.GetVariable("FightResult") as SharedBool;
    }

	public override TaskStatus OnUpdate()
	{
		bool2 result = FightManager.instance.IsFightEnd();
		FightResult.SetValue(result.y);
		if (result.x)
		{
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }
}