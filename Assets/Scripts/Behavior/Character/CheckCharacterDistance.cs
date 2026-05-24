using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("检查角色与目标距离")]
public class CheckCharacterDistance : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt target;
    [SerializeField]
    EntityType entityType;
    [SerializeField]
    private SharedInt distance;

    Character character;
    Character targetCharacter;
    RuntimeMapItem targetItem;
    int oldCharacterId, oldTargetId;
    EntityType oldEntityType;
    public override void OnStart()
    {
        if (oldCharacterId != characterId.Value)
        {
            oldCharacterId = characterId.Value;
            character = CharacterManager.instance.GetCharacter(characterId.Value);
        }
        bool refreshTarget = false;
        if (entityType != oldEntityType)
        {
            oldEntityType = entityType;
            refreshTarget = true;
        }
        if (!refreshTarget)
        {
            if (oldTargetId != target.Value)
            {
                refreshTarget = true;
            }
        }
        if (refreshTarget)
        {
            switch (entityType)
            {
                case EntityType.角色:
                    targetCharacter = CharacterManager.instance.GetCharacter(target.Value);
                    if (targetCharacter != null && targetCharacter.mapInstance == character.mapInstance)
                    {
                        var offsetCoordinate = targetCharacter.coordinate - character.coordinate;
                        int distance = math.abs(offsetCoordinate.x) + math.abs(offsetCoordinate.y);
                        if (distance <= this.distance.Value)
                        {
                            taskStatus = TaskStatus.Success;
                            return;
                        }
                    }
                    break;
                case EntityType.地图道具:
                    if (WorldMapManager.instance.GetRuntimeMapItem(target.Value, out targetItem) &&
                        targetItem.mapInstanceId == character.mapInstance)
                    {
                        var offsetCoordinate = targetItem.coordinate - character.coordinate;
                        int distance = math.abs(offsetCoordinate.x) + math.abs(offsetCoordinate.y);
                        if (distance <= this.distance.Value)
                        {
                            taskStatus = TaskStatus.Success;
                            return;
                        }
                    }
                    break;
            }
        }
        switch (entityType)
        {
            case EntityType.角色:
                if (targetCharacter != null && targetCharacter.mapInstance == character.mapInstance)
                {
                    var offsetCoordinate = targetCharacter.coordinate - character.coordinate;
                    int distance = math.abs(offsetCoordinate.x) + math.abs(offsetCoordinate.y);
                    if (distance <= this.distance.Value)
                    {
                        taskStatus = TaskStatus.Success;
                        return;
                    }
                }
                break;
            case EntityType.地图道具:
                if (targetItem!=null &&
                    targetItem.mapInstanceId == character.mapInstance)
                {
                    var offsetCoordinate = targetItem.coordinate - character.coordinate;
                    int distance = math.abs(offsetCoordinate.x) + math.abs(offsetCoordinate.y);
                    if (distance <= this.distance.Value)
                    {
                        taskStatus = TaskStatus.Success;
                        return;
                    }
                }
                break;
        }

        taskStatus = TaskStatus.Failure;
    }
    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}
