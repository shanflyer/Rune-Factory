using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("Game/Character")]
[TaskName("临时角色展示表情")]
public class TempCharacterShowEmote:Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private int2 showTime;

    TaskStatus taskStatus;
    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        Character character = CharacterManager.instance.GetCharacter(characterId.Value);
        if(character is TempCharacter tempCharacter)
        {
            taskStatus = TaskStatus.Running;
            int emoteId = tempCharacter.GetAreaEmote();
            int showTimeValue = GameRandom.RandomInt(showTime.x, showTime.y);
           // Debug.Log($"emoteId{emoteId}");
            ShowEmote showEmote = new ShowEmote
            {
                emoteId = emoteId,
                id = character.instanceId,
                entityType = EntityType.角色,
                showTime = showTimeValue
            };
            GameActionManager.instance.QueueAction(showEmote);
            GameTimerController.instance.DelayAction(showTimeValue, ShowEmoteEnd);
            return;
        }
        taskStatus = TaskStatus.Failure;
    }
    void ShowEmoteEnd()
    {
        taskStatus = TaskStatus.Success;
    }


    public override TaskStatus OnUpdate()
    {

        return taskStatus;
    }
}
