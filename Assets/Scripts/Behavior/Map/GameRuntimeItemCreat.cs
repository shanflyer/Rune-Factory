using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/Map")]
[TaskName("创建地图道具")]
public class GameRuntimeItemCreat : Action
{ 
	public SharedInt dataId;
	public SharedInt3 coordinate;
	[Header("记录物体id的共享变量")]
	public SharedInt shardId;
	public override void OnStart()
	{ 

		AddMapItem addMapItem = new AddMapItem
		{
			mapId = coordinate.Value.z,
			dataId = dataId.Value,
			coordinate = coordinate.Value.xy, 
		};
        if (shardId != null)
        {
			addMapItem.setValue = SaveNewItem;
		}

		GameActionManager.instance.QueueAction(addMapItem,true);
	}
	void SaveNewItem(int newItemId)
    {
		if (shardId != null)
        {
			shardId.Value = newItemId;
        }

	}
	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}