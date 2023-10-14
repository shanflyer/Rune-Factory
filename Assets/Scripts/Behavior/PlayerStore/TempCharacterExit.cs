using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("Game/PlayerStore")]
[TaskName("消费者离开")]
public class TempCharacterExit : Action
{ 
    private SharedInt characterId;
    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        if (characterId == null)
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (characterId == null)
        {
            taskStatus = TaskStatus.Failure;
            return;
        }
        GetCharacterDataId getCharacterDataId = new GetCharacterDataId
        {
            characterId=characterId.Value,
            SetValue = (int value) =>
            {
                DestoryTempCharacter destoryTempCharacter = new DestoryTempCharacter
                {
                    characterId = characterId.Value,
                    dataId = value
                };
                GameActionManager.instance.QueueAction(destoryTempCharacter);
                taskStatus=TaskStatus.Success; 
            }
        };
        GameActionManager.instance.QueueAction(getCharacterDataId,true);
    }
    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}