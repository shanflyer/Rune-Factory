using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct DynamicData
{
    public SharedInt source, target, value;
}

[TaskCategory("NewGame/Common")]
[TaskName("执行GameAction")]
public class RunGameActions : Action
{ 
    public SharedInt source, target;
    public SharedInt sharedSetIntValue;
    [Header("动态填充数据")]
    public List<DynamicData> otherDatas;

    public List<GameActionData> gameActionDatas;

    void SetValue(int value)
    {
        if (sharedSetIntValue != null)
        {
            sharedSetIntValue.Value = value;
        }
    }
    public override void OnStart()
    {
        for (int i = 0; i < gameActionDatas.Count; i++)
        {
            if (otherDatas != null && i < otherDatas.Count)
            {
                DynamicData otherData = otherDatas[i];
                gameActionDatas[i].Action(otherData.source.Value, otherData.target.Value, otherData.value.Value,
                    setValue: SetValue);
            }
            else
            {
                gameActionDatas[i].Action(source.Value, target.Value, setValue: SetValue);
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}