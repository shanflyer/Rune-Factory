using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[System.Serializable]
public struct DynamicData
{
    public SharedInt source, target;
}
[TaskCategory("NewGame/Common")]
[TaskName("执行GameAction")]
public class RunGameActions : Action
{
    public SharedInt source, target;
    [Header("动态填充数据")]
    public List<DynamicData> otherDatas;
    public List<GameActionData> gameActionDatas;
    public override void OnStart()
    {


        for(int i = 0; i < gameActionDatas.Count; i++)
        {
            if (otherDatas!=null&&i < otherDatas.Count)
            {
                DynamicData otherData = otherDatas[i];
                gameActionDatas[i].Action(otherData.source.Value,otherData.target.Value);
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