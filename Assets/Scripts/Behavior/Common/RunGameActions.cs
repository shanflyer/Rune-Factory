using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DynamicData
{
    public SharedInt source, target, value;
}
[System.Serializable]
public struct DynamicParameterData
{
    public List<SharedVariable> Parameters;
}
[TaskCategory("NewGame/Common")]
[TaskName("执行GameAction")]
public class RunGameActions : Action
{
    public bool immediately;
    public SharedInt source, target;
    public SharedInt sharedSetIntValue;
    [Header("动态填充数据")]
    public List<DynamicData> otherDatas;

    public List<DynamicParameterData> dynamicParameterDatas;

    public List<GameActionData> gameActionDatas;
    public bool waitResult;

    TaskStatus taskStatus = TaskStatus.Running;
    void SetValue(int value)
    {
        if (sharedSetIntValue != null)
        {
            sharedSetIntValue.Value = value;
        }
    }
    void SetActionResult(bool result)
    {
        if (result)
        {
            taskStatus = TaskStatus.Success;
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }
    }
    public override void OnStart()
    {
        if(waitResult)
        {
            taskStatus = TaskStatus.Running;
        }
        else
        {
            taskStatus = TaskStatus.Success;
        }
        for (int i = 0; i < gameActionDatas.Count; i++)
        {
            if (dynamicParameterDatas != null && i < dynamicParameterDatas.Count)
            {
                var dynamicParameterData = dynamicParameterDatas[i];
                var gameActionData = gameActionDatas[i];
                List<Parameter> _parameters = new List<Parameter>();
                for(int j = 0; j < dynamicParameterData.Parameters.Count; j++)
                {
                    Parameter parameter = new Parameter
                    {
                        value = dynamicParameterData.Parameters[j].ToString()
                    };
                    _parameters.Add(parameter);
                }
                gameActionData._parameters= _parameters;
             }

            if (otherDatas != null && i < otherDatas.Count)
            {
                DynamicData otherData = otherDatas[i];
                gameActionDatas[i].Action(otherData.source.Value, otherData.target.Value, otherData.value.Value,
                   setResult:waitResult? SetActionResult:null,setValue: SetValue, immediately: immediately);
            }
            else
            {
                gameActionDatas[i].Action(source.Value, target.Value, setResult: waitResult ? SetActionResult : null,
                    setValue: SetValue, immediately: immediately);
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}