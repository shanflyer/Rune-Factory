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
    public SharedInt3List results;
    public SharedInt3 target;
    public override void OnStart()
    {
        int index = GameRandom.RandomInt(0, results.Value.Count);
        target = results.Value[index];
    }

    public override TaskStatus OnUpdate()
    { 
        return TaskStatus.Success;
    }
}
