using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks; 

[TaskCategory("NewGame/Common")]
[TaskName("执行GameAction")]
public class RunGameActions : Action
{
    public List<GameActionData> gameActionDatas;
    public override void OnStart()
    {
        for(int i = 0; i < gameActionDatas.Count; i++)
        {
            gameActionDatas[i].Action();
        }
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;

    }
}