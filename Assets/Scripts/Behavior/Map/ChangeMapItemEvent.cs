using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("Game/Map")]
[TaskName("改变地图道具")]
public class ChangeMapItemEvent : Action
{
	public SharedInt itemId;
	public SharedInt newDataId;
	public SharedVector2Int animationKey;
	public bool immediately = false;
	public override void OnStart()
	{
		ChangeMapItem changeMapItem = new ChangeMapItem
		{
			itemId = itemId.Value,
			newDataId = newDataId.Value,
			animationKey=new int2(animationKey.Value.x, animationKey.Value.y)
		};
		GameActionManager.instance.QueueAction(changeMapItem, immediately);
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}
