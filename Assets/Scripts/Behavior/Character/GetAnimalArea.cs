using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;


[TaskCategory("Game/Character")]
[TaskName("获取动物区域")]
public class GetAnimalArea : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt3 result;
    [SerializeField]
    private BehaviorAreaType behaviorAreaType;
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
            if (animal.pasture != 0)
            {
                var cell = MapCellController.instance.GetRandomBehaviorCell(animal.pasture, behaviorAreaType);
                result.SetValue(new int3(cell.xy, animal.pasture));
                return TaskStatus.Success;
            }

        }

        return TaskStatus.Failure;
    }
}
