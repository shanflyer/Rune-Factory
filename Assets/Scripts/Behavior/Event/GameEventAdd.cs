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
		AsyncTaskRunner.Run(OnStartAsync, nameof(GameEventAdd));
	}

	private async System.Threading.Tasks.Task OnStartAsync()
	{
	  await	GameEventManager.instance.AddGameEvent(gameEventId.Value);
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}
