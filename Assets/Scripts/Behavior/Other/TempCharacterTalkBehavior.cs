using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Game/Character")]
[TaskName("临时角色说话")]
public class TempCharacterTalkBehavior : Action
{
    [SerializeField]
    private SharedInt characterId;
    [SerializeField]
    private SharedInt TargetCharacter;
    [SerializeField]
    private SharedBool isStopMove;
    [SerializeField]
    private SharedBool faceTarget;
    private void CharacterStartMoveAction()
    {
        StartCharacterMove startCharacterMove = new StartCharacterMove
        {
            characterId = characterId.Value
        };
        GameActionManager.instance.QueueAction(startCharacterMove);
    }

    private void RemoveEvent()
    {
        var ID = (SharedInt)Owner.GetVariable("ID");
        RemoveGameEvent RemoveGameEvent = new RemoveGameEvent
        {
            eventId = ID.Value
        };
        GameActionManager.instance.QueueAction(RemoveGameEvent, true);
    }

    public override void OnStart()
    {
        var source = CharacterManager.instance.GetCharacter(characterId.Value); 

        if (source is TempCharacter tempCharacter)
        {
            if (faceTarget.Value && TargetCharacter != null)
            {
                Character character = CharacterManager.instance.GetCharacter(TargetCharacter.Value);
                if (character != null)
                {
                    SetTargetDirection setTargetDirection = new SetTargetDirection
                    {
                        characterId = characterId.Value,
                        targetCoordinate = character.coordinate
                    };
                    GameActionManager.instance.QueueAction(setTargetDirection, true);
                }
            }

            if (isStopMove.Value)
            {
                StopCharacterMove stopCharacterMove = new StopCharacterMove
                {
                    characterId = characterId.Value
                };
                GameActionManager.instance.QueueAction(stopCharacterMove, true);
            }

            TempCharacterTalk tempCharacterTalk = new TempCharacterTalk
            {
                characterId = characterId.Value,
                endAction = isStopMove.Value ? CharacterStartMoveAction : null
            };
          
            GameActionManager.instance.QueueAction(tempCharacterTalk);

            taskStatus = TaskStatus.Success;
        }
        else
        {
            taskStatus = TaskStatus.Failure;
        }

      
    }

    private TaskStatus taskStatus;

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}