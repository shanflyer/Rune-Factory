using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("获得渔夫位置")]
public class GetFisherToolCoordinateBehavior : Action
{
    public SharedInt characterId;
    [SerializeField]
    private SharedInt3 coordinate;  

    public override void OnStart()
    { 
    } 

    public override TaskStatus OnUpdate()
    {
        if (FishController.instance.GetFisherToolCoordinate(characterId.Value,out var _coordinate))
        {
            coordinate.SetValue(_coordinate);
            return TaskStatus.Success;
        }
        return TaskStatus.Failure; 
    }
}