using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Map")]
[TaskName("获取地图特定行为类型的随机格子")]
public class GetRandomMapCell : Action
{
    [SerializeField]
    private SharedInt room;

    [SerializeField]
    private BehaviorAreaType behaviorAreaType;

    [Header("获取的结果")]
    [SerializeField]
    private SharedInt3 result;

    public override void OnStart()
    {
    }

    public override TaskStatus OnUpdate()
    {
        var cell = MapCellController.instance.GetRandomBehavioCell(room.Value, behaviorAreaType);
        result.SetValue(new int3(cell.xy, room.Value));
        return TaskStatus.Success;
    }
}