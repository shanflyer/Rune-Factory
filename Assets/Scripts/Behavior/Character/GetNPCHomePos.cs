using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;


[TaskCategory("Game/Character")]
[TaskName("获取角色家地图位置")]
public class GetNPCHomePos : Action
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
            int visitMap = npc.HomeMap;

            if (visitMap > 0)
            {
                int areaId = npc.GetHomeArea(); 
                if (areaId == 0)
                {
                    try
                    {
                        var cell = MapCellController.instance.GetRandomBehaviorCell(visitMap, BehaviorAreaType.聚集);
                        result.SetValue(new int3(cell.xy, visitMap));
                    }
                    catch
                    {
                        Debug.LogError($"npc-{Owner.BehaviorName} error");
                        return TaskStatus.Failure;
                    }
                }
                else
                {
                    var cell = MapCellController.instance.GetRandomBehaviorCell(visitMap, areaId);
                    result.SetValue(new int3(cell.xy, visitMap));
                }
                return TaskStatus.Success;
            }
        }

        return TaskStatus.Failure;
    }
}