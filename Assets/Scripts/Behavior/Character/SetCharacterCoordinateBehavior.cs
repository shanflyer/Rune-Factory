using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("Game/Character")]
[TaskName("设置角色位置")] 

public class SetCharacterCoordinateBehavior:Action
{
    public SharedInt characterId;
    public SharedObjCoordinate coordinate; 
    public override void OnStart()
    {
        SetCharacterCoordinate setCharacterCoordinate = new SetCharacterCoordinate
        {
            characterId = characterId.Value,
            mapId = coordinate.Value.mapInstance,
            coordinate =new int2(coordinate.Value.x, coordinate.Value.y)
        };
        GameActionManager.instance.QueueAction(setCharacterCoordinate,true);
    }
    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}