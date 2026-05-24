using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/Character")]
[TaskName("设置、改变角色属性")]
public class CharacterPropertyEvent : Action
{
	public SharedInt characterId;
	public CharacterPropertyType characterPropertyType;
	public SharedInt value;
	[Header("改变而不是设置")]
	public SharedBool change;
	public override void OnStart()
	{
        if (change.Value)
        {
			ChangeCharacterProperty changeCharacterProperty = new ChangeCharacterProperty
			{
				characterId=characterId.Value,
				propertyType= characterPropertyType,
				changeValue=value.Value
			};
			GameActionManager.instance.QueueAction(changeCharacterProperty);

        }
        else
        {
			SetCharacterProperty changeCharacterProperty = new SetCharacterProperty
			{
				characterId = characterId.Value,
				propertyType = characterPropertyType,
				Value = value.Value
			};
			GameActionManager.instance.QueueAction(changeCharacterProperty);
		}
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}
