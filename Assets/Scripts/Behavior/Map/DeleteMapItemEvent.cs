using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/Map")]
[TaskName("删除地图道具")]
public class DeleteMapItemEvent : Action
{
	public SharedInt mapItemId;
	public SharedBool triggerClear;
	public override void OnStart()
	{
		DeleteMapItem deleteMapItem = new DeleteMapItem
		{
			mapItemInstanceId = mapItemId.Value,
			triggerClear=triggerClear.Value
		};
		GameActionManager.instance.QueueAction(deleteMapItem,true);
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}
