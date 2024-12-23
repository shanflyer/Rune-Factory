using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("Game/Map")]
[TaskName("使用物体链接数据偏移坐标")]
public class OffsetCoordinateForItemLinkPos : Action
{
    [SerializeField]
    private SharedInt item;
    [SerializeField]
    private SharedInt3 source;
    [SerializeField]
    private SharedInt3 target;
    public override void OnStart()
    {
        if(WorldMapManager.instance.GetRuntimeMapItem(item.Value,out var runtimeMapItem))
        {
            var offsetPos = runtimeMapItem.mapItemData.offsetLinkPos;
            int3 targetCoordinate = new int3(source.Value.x + (int)offsetPos.x, source.Value.y + (int)offsetPos.y, source.Value.z);
            target.Value = targetCoordinate;
        }
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}