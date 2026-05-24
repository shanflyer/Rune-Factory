using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;
using UnityEngine;

[TaskCategory("Game/Map")]
[TaskName("获取地图特定行为类型的随机格子")]
public class GetRandomMapCell : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt room;
    [SerializeField]
    private SharedInt areaId;
    [SerializeField]
    private BehaviorAreaType behaviorAreaType;

    private readonly bool fixedArea = false;

    [Header("获取的结果")]
    [SerializeField]
    private SharedInt3 result;

    public override void OnStart()
    {
    }

    public override TaskStatus OnUpdate()
    {
        int3 cell = int3.zero;
        if (room != null&& !room.IsNull())
        {
            if (!fixedArea)
                cell = MapCellController.instance.GetRandomBehaviorCell(room.Value, behaviorAreaType);
            else
                cell = MapCellController.instance.GetRandomBehaviorCell(room.Value, areaId.Value);

            result.SetValue(new int3(cell.xy, room.Value));
            // areaId.SetValue(cell.z);
            return TaskStatus.Success;
        }


        if (characterId != null)
        {
            Character character = CharacterManager.instance.GetCharacter(characterId.Value);
            if (cell.x == 0)
            {
                if (!fixedArea)
                    cell = MapCellController.instance.GetRandomBehaviorCell(character.mapInstance, behaviorAreaType);
                else
                    cell = MapCellController.instance.GetRandomBehaviorCell(character.mapInstance, areaId.Value);

                result.SetValue(new int3(cell.xy, character.mapInstance));
                // areaId.SetValue(cell.z);
            }
            if(character is TempCharacter tempCharacter)
            {
                tempCharacter.SetTargetArea(areaId.Value,cell.xy);
            }
        }


        return TaskStatus.Success;
    }
}
