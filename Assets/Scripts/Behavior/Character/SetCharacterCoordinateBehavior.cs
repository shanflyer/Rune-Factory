using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("Game/Character")]
[TaskName("设置角色位置")] 

public class SetCharacterCoordinateBehavior:Action
{
    public SharedInt characterId;
    public SharedInt3 coordinate; 
    public override void OnStart()
    {
        SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
        {
            characterId = characterId.Value, 
            coordinate = coordinate.Value
        };
        GameActionManager.instance.QueueAction(setCharacterCoordinate,true);
    }
    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}