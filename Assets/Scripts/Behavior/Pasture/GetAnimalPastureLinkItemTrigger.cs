using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;


[TaskCategory("Game/Character")]
[TaskName("获取动物所在的畜牧场院子")]
public class GetAnimalPastureLinkItemTrigger : Action
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
        if (PastureManager.instance.GetAnimal(characterId.Value, out var animal))
        {
            if(PastureManager.instance.GetPasture(animal.pasture,out var pasture))
            {
               if(WorldMapManager.instance.GetRuntimeMapItem(pasture.linkItem,out var runtimeMapItem))
                {
                    var cells = MapCellController.instance.GetItemTriggerCells(pasture.linkItem, runtimeMapItem.mapInstanceId);
                    if (cells.Length > 0)
                    {
                        int index = GameRandom.RandomInt(0, cells.Length);
                        result.SetValue(new int3(cells[index], runtimeMapItem.mapInstanceId));
                        return TaskStatus.Success;
                    }
                }
             
            } 
        }

        return TaskStatus.Failure;
    }
}