using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

public enum CellType
{
    CommonTrigger,PlayerTrigger
}
[TaskCategory("Game/Character")]
[TaskName("获取距离角色最近的物体格子")]
public class GetCharacterToMapObjNearestCell : Action
{
    private SharedInt characterId;
    public SharedInt itemId;
    public SharedInt3 targetCoordinate;
    public CellType cellType;
    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (targetCoordinate == null || targetCoordinate.IsNull())
        {
            targetCoordinate = (SharedInt3)Owner.GetVariable("TargetCoordinate");
            if (targetCoordinate == null || targetCoordinate.IsNull())
            {
                targetCoordinate = new SharedInt3();
                Owner.SetVariable("TargetCoordinate", targetCoordinate);
            }
        } 
    }
     
    public override TaskStatus OnUpdate()
    {
        var character = CharacterManager.instance.GetCharacter(characterId.Value);
        if (character == null)
        {
            return TaskStatus.Failure;
        }
        var result = WorldMapManager.instance.GetNearestItemCell(itemId.Value, character.coordinate,cellType);

        targetCoordinate.SetValue(new int3(result.xy,character.mapInstance));

        return TaskStatus.Success;
    }
}