using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Map")]
[TaskName("获取物体特定格子")]
public class GetMapItemTriggerCells : Action
{
    public SharedInt2List cells;

    public SharedInt  mapItemInstance;

    public override void OnStart()
    {
    }

    public override TaskStatus OnUpdate()
    {



        return TaskStatus.Success;
    }
}
