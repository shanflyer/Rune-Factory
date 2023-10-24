using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
[TaskCategory("NewGame/Common")]
[TaskName("检查道具值")]
public class CheckItemValueBehavior : Action
{
    public SharedInt characterId;
    public SharedInt packageId; 
    public SharedInt itemDataId;
    public SharedInt itemValue;

    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        int package = packageId.Value;
        if (!characterId.IsNull())
        {
            Character character = CharacterManager.instance.GetCharacter(characterId.Value);
            if (character != null)
            {
                package = character.characterPackage;
            }
        }
        CheckItemValue checkItemValue = new CheckItemValue
        {
            packageId = package,
            itemDataId = itemDataId.Value, 
            itemValue = itemValue.Value,
            setResult=SetResult
        };
        GameActionManager.instance.QueueAction(checkItemValue);
    }
    void SetResult(bool result)
    {
        if (result)
        {
            taskStatus = TaskStatus.Success;
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }
    }
    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    { 
        return taskStatus; 
    }
}