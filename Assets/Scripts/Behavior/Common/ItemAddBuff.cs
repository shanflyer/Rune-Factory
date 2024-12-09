
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("NewGame/Common")]
[TaskName("道具增加Buff")]
public class ItemAddBuff : Action
{
    [SerializeField]
    SharedInt characterId;
    [SerializeField]
    SharedInt itemId;
    public override async void OnStart()
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(itemId.Value);

        var property = itemData.Property;
        if (property.Other != 0)
        {
            if (property.AT != 0)
            {

            }
        }
         
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}