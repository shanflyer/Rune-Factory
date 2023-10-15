using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("Game/Map")]
[TaskName("创建地图道具")]
public class GameRuntimeItemCreat : Action
{
	public SharedInt mapId;
	public SharedInt dataId;
	public SharedVector2Int coordinate;
	[Header("记录物体id的共享变量")]
	public SharedInt shardId;
	public override void OnStart()
	{
		int trueMapId = mapId.Value;
        if (mapId==null|| mapId.IsNull() ||mapId.Value == -1)
        {
			trueMapId = WorldMapManager.instance.displayMap;
        }

		AddMapItem addMapItem = new AddMapItem
		{
			mapId = trueMapId,
			dataId = dataId.Value,
			coordinate =new Unity.Mathematics.int2(coordinate.Value.x, coordinate.Value.y), 
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