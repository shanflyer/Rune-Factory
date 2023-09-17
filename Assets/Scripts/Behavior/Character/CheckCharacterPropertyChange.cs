using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/Character")]
[TaskName("检查角色属性改变")]
[TaskIcon("{SkinColor}SelectorIcon.png")]
public class CheckCharacterPropertyChange:Action
{
    public CompareTarget source;
    public CompareType compareType;
    public CompareTarget target;

    public bool continued;

    private int sourceValue,targetValue;
    private bool result;
    private bool addAction;
    private void PropertyChangeAction(CharacterPropertyTrigger characterPropertyTrigger)
    {
        sourceValue = source.value;
        targetValue = target.value;
        if (source.propertyType != CharacterPropertyType.自定义值)
        {
            if (characterPropertyTrigger.characterId == source.characterId.Value)
            {
                sourceValue = characterPropertyTrigger.characterProperty.GetValue(source.propertyType); 
            }
        }
        if (target.propertyType != CharacterPropertyType.自定义值)
        {
            if (characterPropertyTrigger.characterId == target.characterId.Value)
            { 
                targetValue = characterPropertyTrigger.characterProperty.GetValue(target.propertyType);
            }
        }
		switch (compareType)
		{
			case CompareType.等于:
				result = sourceValue == targetValue; 
				break;
			case CompareType.不等于:
				result = sourceValue != targetValue;
				break;
			case CompareType.大于:
				result = sourceValue > targetValue;
				break;
			case CompareType.不大于:
				result = sourceValue! > targetValue;
                break;
			case CompareType.小于:
				result = sourceValue < targetValue;
                break;
			case CompareType.不小于:
				result = sourceValue! < targetValue;
                break;

		}

	}
    public override void OnStart()
    {
        if (!addAction)
        {
            GameActionManager.instance.AddListener<CharacterPropertyTrigger>(PropertyChangeAction);
            addAction = true;
        }
    }

    public override TaskStatus OnUpdate()
    {
        if (result)
        {
            return TaskStatus.Success;
        }
        if (!continued)
        {
            result = false;
        }
        return TaskStatus.Failure;
    }
}