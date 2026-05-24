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
		if (gameEventId.IsNull())
		{
            var behavior = this.Owner;
			gameEventId = (SharedInt)behavior.GetVariable("ID");
        }

		GameEventManager.instance.RemoveGameEvent(gameEventId.Value);
		if(Owner)
            GameObject.Destroy(Owner);
    }

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}
