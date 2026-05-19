using System;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

[Serializable]
public class DynamicData
{
    public SharedInt source, target, value;
}

[Serializable]
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

    public List<GameActionAsset> gameActionDatas;
    public bool waitResult;

    private TaskStatus taskStatus = TaskStatus.Running;

    private void SetValue(int value)
    {
        if (sharedSetIntValue != null)
        {
            sharedSetIntValue.Value = value;
        }
    }

    private void SetActionResult(bool result)
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
        if (waitResult)
        {
            taskStatus = TaskStatus.Running;
        }
        else
        {
            taskStatus = TaskStatus.Success;
        }
        if (gameActionDatas == null) return;

        for (int i = 0; i < gameActionDatas.Count; i++)
        {
            var gameActionData = gameActionDatas[i];
            if (gameActionData == null) continue;

            List<Parameter> runtimeParameters = null;
            if (dynamicParameterDatas != null && i < dynamicParameterDatas.Count)
            {
                var dynamicParameterData = dynamicParameterDatas[i];
                runtimeParameters = new List<Parameter>();
                if (dynamicParameterData.Parameters != null)
                {
                    for (int j = 0; j < dynamicParameterData.Parameters.Count; j++)
                    {
                        Parameter parameter = new Parameter
                        {
                            value = dynamicParameterData.Parameters[j].ToString()
                        };
                        runtimeParameters.Add(parameter);
                    }
                }
            }

            if (GameDataManager.instance.GlobalData.debug)
                Debug.Log(
                    $"behaviorAction:{gameActionData.root?.GetType().Name}--BehaviorName:{Owner.BehaviorName}--{FriendlyName}");

            if (otherDatas != null && i < otherDatas.Count)
            {
                DynamicData otherData = otherDatas[i];
                if (runtimeParameters != null)
                {
                    gameActionData.CreateAction(runtimeParameters, otherData.source.Value, otherData.target.Value, otherData.value.Value,
                        setResult: waitResult ? SetActionResult : null, setValue: SetValue, immediately: immediately);
                }
                else
                {
                    gameActionData.Action(otherData.source.Value, otherData.target.Value, otherData.value.Value,
                        setResult: waitResult ? SetActionResult : null, setValue: SetValue, immediately: immediately);
                }
            }
            else
            {
                if (runtimeParameters != null)
                {
                    gameActionData.CreateAction(runtimeParameters, source.Value, target.Value, sharedSetIntValue.Value,
                        setResult: waitResult ? SetActionResult : null, setValue: SetValue, immediately: immediately);
                }
                else
                {
                    gameActionData.Action(source.Value, target.Value, sharedSetIntValue.Value,
                        setResult: waitResult ? SetActionResult : null, setValue: SetValue, immediately: immediately);
                }
            }
            
        }
    }

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}
