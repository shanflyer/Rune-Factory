using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/Map")]
[TaskName("创建触发格子")]
public class CreatTriggerCell : Action
{
	public SharedCoordinateArray triggerCells;
	public SharedInt room;
	public SharedInt enterEvent, exitEvent;
	public SharedInt linkId;
	public EntityType triggerType;
	public override void OnStart()
	{
		MapCellController.instance.AddTriggerCell(triggerCells.Value, room.Value, enterEvent.Value, exitEvent.Value,
			triggerType,linkId.Value, Unity.Mathematics.int2.zero);
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}
