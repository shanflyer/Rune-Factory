using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/Character")]
[TaskName("设置角色移动目标")]
public class SetCharacterMoveTarget : Action
{
    [Header("获取的结果")]
    private SharedInt3List results;
    public SharedInt3 targetCoordinate;
    public override void OnStart()
    {
        if (targetCoordinate==null|| targetCoordinate.IsNull())
        {
            targetCoordinate=(SharedInt3)Owner.GetVariable("TargetCoordinate");
            if (targetCoordinate==null|| targetCoordinate.IsNull())
            {
                targetCoordinate = new SharedInt3();
                Owner.SetVariable("TargetCoordinate", targetCoordinate);
            }
        }

        if (results == null)
        {
            results = (SharedInt3List)Owner.GetVariable("CoordinateResults");
            if (results == null)
            {
                results = new SharedInt3List();
                Owner.SetVariable("CoordinateResults", results);
            }
        }

        int index = GameRandom.RandomInt(0, results.Value.Count);
        targetCoordinate.SetValue(results.Value[index]);
    }

    public override TaskStatus OnUpdate()
    { 
        return TaskStatus.Success;
    }
}
