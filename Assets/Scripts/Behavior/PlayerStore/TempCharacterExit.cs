using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("销毁临时角色")]
public class TempCharacterExit : Action
{
    [SerializeField]
    private SharedInt characterId;

    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (characterId == null || characterId.IsNull())
        {
            taskStatus = TaskStatus.Failure;
            return;
        }
        GetCharacterDataId getCharacterDataId = new GetCharacterDataId
        {
            characterId = characterId.Value,
            SetValue = (int value) =>
            {
                DestoryCharacter destoryTempCharacter = new DestoryCharacter
                {
                    characterId = characterId.Value,
                    dataId = value,
                    isTemp = true
                };
                GameActionManager.instance.QueueAction(destoryTempCharacter);
                taskStatus = TaskStatus.Success;
            }
        };
        GameActionManager.instance.QueueAction(getCharacterDataId, true);
    }

    private TaskStatus taskStatus;

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}