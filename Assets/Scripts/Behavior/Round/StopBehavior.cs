using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/回合/事件")]
[TaskName("停止行为")]
public class StopBehavior : Action
{ 

    public override void OnStart()
    {
        Owner.DisableBehavior();
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}