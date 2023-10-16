using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("NewGame/Common")]
[TaskName("执行GameAction")]
public class RunGameActions : Action
{
    public SharedInt source, target;
    public List<int2> otherDatas;
    public List<GameActionData> gameActionDatas;
    public override void OnStart()
    {


        for(int i = 0; i < gameActionDatas.Count; i++)
        {
            if (otherDatas!=null&&i < otherDatas.Count)
            {
                int2 otherData = otherDatas[i];
                gameActionDatas[i].Action(otherData.x,otherData.y);
            }
            else
            {
                gameActionDatas[i].Action();
            }
           
        }
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;

    }
}