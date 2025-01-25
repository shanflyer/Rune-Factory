using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/Common")]
[TaskName("检查是否直接开始游戏")]
public class CheckGameStartPlay : Action
{ 


    public override void OnStart()
    {

    }

    public override TaskStatus OnUpdate()
    { 
        if (GameController.instance.startPlay)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;

    }
}