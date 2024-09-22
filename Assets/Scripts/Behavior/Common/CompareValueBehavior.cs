using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

[TaskCategory("NewGame/Common")]
[TaskName("检查数值")]
public class CompareValueBehavior : Action
{
    [SerializeField]
    public SharedInt source;
    public CompareType compareType;
    [SerializeField]
    public SharedInt target;
    public override void OnStart()
    { 
    }

    public override TaskStatus OnUpdate()
    {
        switch (compareType)
        {
            case CompareType.等于:
                if (source.Value == target.Value)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.不等于:
                if (source.Value != target.Value)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.大于:
                if (source.Value > target.Value)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.不大于:
                if (source.Value! > target.Value)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.小于:
                if (source.Value < target.Value)
                {
                    return TaskStatus.Success;
                }
                break;
            case CompareType.不小于:
                if (source.Value! < target.Value)
                {
                    return TaskStatus.Success;
                }
                break;
        }
        return TaskStatus.Failure;
    }
}