using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;


[TaskCategory("Game/Character")]
[TaskName("获取角色工作地图位置")]
public class GetNPCWorkPos : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt3 result;
    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (NPCManager.instance.GetNPCFormInstance(characterId.Value, out var npc))
        {
            int visitMap = npc.WorkMap;

            if (visitMap > 0)
            {
                int areaId = npc.GetWorkArea();
                if (areaId == 0)
                {
                    var cell = MapCellController.instance.GetRandomBehavioCell(visitMap, BehaviorAreaType.聚集);
                    result.SetValue(new int3(cell.xy, visitMap));
                }
                else
                {
                    var cell = MapCellController.instance.GetRandomBehavioCell(visitMap, areaId);
                    result.SetValue(new int3(cell.xy, visitMap));
                }
                return TaskStatus.Success;
            }
        }
        else if(PastureManager.instance.GetAnimal(characterId.Value,out var animal))
        {
            int room = animal.GetPastureRoom();
            if (room!=0&&!TeamManager.instance.IsInTeam(characterId.Value))
            {
                var cell = MapCellController.instance.GetRandomBehavioCell(room, BehaviorAreaType.聚集);
                result.SetValue(new int3(cell.xy, room));
                return TaskStatus.Success;
            }
        }

        return TaskStatus.Failure;
    }
}