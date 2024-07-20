using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using UnityEngine;

[TaskCategory("Game/PlayerStore")]
[TaskName("玩家商店是否开业")]
public class CheckPlayerStore : Action
{  

    public override TaskStatus OnUpdate()
    { 
        if (PlayerStoreManager.instance.playerStoreOpen)
        {
            // Debug.Log("target:" + target);
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}