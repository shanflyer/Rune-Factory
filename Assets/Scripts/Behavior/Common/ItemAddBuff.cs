
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
        AddBuffAction addBuffAction = default(AddBuffAction);
        addBuffAction.characterId = characterId.Value;
        var property = itemData.Property;
        if (property.Other != 0)
        {
            addBuffAction.overrideLifeTime = property.Other;
            if (property.AT != 0)
            {
                int2 overrideAddValue = new int2((int)CharacterPropertyType.攻击, property.AT);
                addBuffAction.buffDataId = GameCommon.AddATBuff;
                addBuffAction.overrideAddValue = overrideAddValue;
                GameActionManager.instance.QueueAction(addBuffAction);
            }
            if (property.DF != 0)
            {
                int2 overrideAddValue = new int2((int)CharacterPropertyType.防御, property.DF);
                addBuffAction.buffDataId = GameCommon.AddDFBuff;
                addBuffAction.overrideAddValue = overrideAddValue;
                GameActionManager.instance.QueueAction(addBuffAction);
            }
            if (property.Speed != 0)
            {
                int2 overrideAddValue = new int2((int)CharacterPropertyType.敏捷, property.Speed);
                addBuffAction.buffDataId = GameCommon.AddSpeedBuff;
                addBuffAction.overrideAddValue = overrideAddValue;
                GameActionManager.instance.QueueAction(addBuffAction);
            }
            if (property.Lucky != 0)
            {
                int2 overrideAddValue = new int2((int)CharacterPropertyType.幸运, property.Lucky);
                addBuffAction.buffDataId = GameCommon.AddLuckyBuff;
                addBuffAction.overrideAddValue = overrideAddValue;
                GameActionManager.instance.QueueAction(addBuffAction);
            }
        }
         
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}