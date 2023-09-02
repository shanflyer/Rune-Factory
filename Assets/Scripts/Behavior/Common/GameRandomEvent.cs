using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/Common")]
[TaskName("Ëæ»ú½á¹û")]
public class GameRandomEvent : Action
{
	public SharedInt randomId;
	public SharedRandomResults randomResults;
	public override void OnStart()
	{
		var results = GameRandom.instance.GetRandomValue(randomId.Value);
		randomResults.Value = results;
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}