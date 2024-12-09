
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("NewGame/Common")]
[TaskName("道具增加属性值")]
public class ItemAddPropertyValue : Action
{
    [SerializeField]
    SharedInt characterId;
    [SerializeField]
    SharedInt itemId; 
    public override async void OnStart()
    {
        ItemData itemData = await GameDataManager.instance.GetAsyncData<ItemData>(itemId.Value);
      
        var property = itemData.Property;
        if (property.HP != 0)
        {
            ChangeCharacterProperty changeCharacterProperty = new ChangeCharacterProperty
            {
                characterId = characterId.Value,
                propertyType = CharacterPropertyType.生命,
                changeValue = property.HP
            };
            GameActionManager.instance.QueueAction(changeCharacterProperty);
        }
        if (property.MP != 0)
        {
            ChangeCharacterProperty changeCharacterProperty = new ChangeCharacterProperty
            {
                characterId = characterId.Value,
                propertyType = CharacterPropertyType.法力,
                changeValue = property.MP
            };
            GameActionManager.instance.QueueAction(changeCharacterProperty);
        }
        if (property.Power != 0)
        {
            ChangeCharacterProperty changeCharacterProperty = new ChangeCharacterProperty
            {
                characterId = characterId.Value,
                propertyType = CharacterPropertyType.体力,
                changeValue = property.Power
            };
            GameActionManager.instance.QueueAction(changeCharacterProperty);
        }
    }
 
    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}