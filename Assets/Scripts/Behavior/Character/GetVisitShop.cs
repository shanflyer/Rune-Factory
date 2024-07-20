using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;


[TaskCategory("Game/Character")]
[TaskName("获取要拜访的商店")]
public class GetVisitShop : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField] private SharedInt visitShop;
    [SerializeField]
    private SharedInt visitMap;
    [SerializeField]
    private SharedInt shopItemId;
    [SerializeField]
    private SharedInt3 targetCell;
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
            int visitShop = npc.GetVisitShop();
            this.visitShop.SetValue(visitShop);
            if (visitShop > 0)
            {

                var mapItem = ShopManager.instance.GetShopMapItem(visitShop);
                visitMap.SetValue(mapItem.x);
                shopItemId.SetValue(mapItem.y);

                var cell = WorldMapManager.instance.GetRandomItemPlayerTriggerCell(mapItem.x, mapItem.y);
                targetCell.Value = new int3(cell, visitMap.Value);
                return TaskStatus.Success;
            }
        }

        return TaskStatus.Failure;
    }
}