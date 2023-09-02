using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/Event")]
[TaskName("添加事件")]
public class GameEventAdd : Action
{
	public SharedInt gameEventId; 
	public override void OnStart()
	{
		GameEventManager.instance.AddGameEvent(gameEventId.Value);
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}