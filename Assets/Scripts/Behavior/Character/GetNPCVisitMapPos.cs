using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;


[TaskCategory("Game/Character")]
[TaskName("获取角色要拜访的地图位置")]
public class GetNPCVisitMapPos : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField] private SharedInt visitMap;
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
        if (NPCManager.instance.GetNPCFormInstance(characterId.Value,out var npc))
        { 
            int visitMap = npc.GetVisitMap();
            this.visitMap.SetValue(visitMap);
            if (visitMap > 0)
            {
                var cell = MapCellController.instance.GetRandomBehavioCell(visitMap, BehaviorAreaType.聚集);
                result.SetValue(new int3(cell.xy, visitMap));
                
                return TaskStatus.Success;
            } 
        } 
         
        return TaskStatus.Failure;
    }
}