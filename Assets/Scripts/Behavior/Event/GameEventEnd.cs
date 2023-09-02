using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/Event")]
[TaskName("事件结束移除")]
public class GameEventEnd : Action
{
	public SharedInt gameEventId;
	public override void OnStart()
	{
		//var behavior = this.Owner;
		//var id=(int)behavior.GetVariable("ID").GetValue();
		GameEventManager.instance.RemoveGameEvent(gameEventId.Value);
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}