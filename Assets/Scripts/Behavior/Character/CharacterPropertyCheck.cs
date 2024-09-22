using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/Character")]
[TaskName("检查角色属性")]
[TaskIcon("{SkinColor}SelectorIcon.png")]
public class CharacterPropertyCheck : Action
{ 
	public CompareTarget source;
	public CompareType compareType;
	public CompareTarget target;
	 
	// Start is called before the first frame update
	public override void OnStart()
	{
		
	} 

	public override TaskStatus OnUpdate()
	{
		int sourceValue = source.value;
		if (source.propertyType != CharacterPropertyType.自定义值)
		{
			sourceValue = CharacterManager.instance.GetCharacterProperty(source.characterId.Value, source.propertyType);
		}
		int targetValue = target.value;
		if (target.propertyType != CharacterPropertyType.自定义值)
		{
			targetValue = CharacterManager.instance.GetCharacterProperty(target.characterId.Value, target.propertyType);
		}
		switch (compareType)
		{
			case CompareType.等于:
				if (sourceValue == targetValue)
				{
					return TaskStatus.Success;
				}
				break;
			case CompareType.不等于:
				if (sourceValue != targetValue)
				{
					return TaskStatus.Success;
				}
				break;
			case CompareType.大于:
				if (sourceValue > targetValue)
				{
					return TaskStatus.Success;
				}
				break;
			case CompareType.不大于:
				if (sourceValue<= targetValue)
				{
					return TaskStatus.Success;
				}
				break;
			case CompareType.小于:
				if (sourceValue < targetValue)
				{
					return TaskStatus.Success;
				}
				break;
			case CompareType.不小于:
				if (sourceValue>= targetValue)
				{
					return TaskStatus.Success;
				}
				break;

		}


		return TaskStatus.Failure;
	}
}

[System.Serializable]
public struct CompareTarget
{
	public SharedInt characterId;
	public CharacterPropertyType propertyType;
	public int value;
	public bool initInt;

}