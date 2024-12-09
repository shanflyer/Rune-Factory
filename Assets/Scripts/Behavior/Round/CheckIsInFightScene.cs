using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using static BehaviorDesigner.Runtime.BehaviorManager;
using Unity.Mathematics;

[TaskCategory("NewGame/回合")]
[TaskName("检查是否处在战斗场景")]
public class CheckIsInFightScene : Action
{ 
    public override void OnStart()
    {
        
    }

    public override TaskStatus OnUpdate()
    { 
        if (ExploreManager.instance.isExplore)
        {
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
    }
}