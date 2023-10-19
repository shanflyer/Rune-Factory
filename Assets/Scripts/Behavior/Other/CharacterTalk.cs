using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;

[TaskCategory("Game/Character")]
[TaskName("角色说话")]
public class CharacterTalk : Action
{
    private SharedInt characterId;
    public SharedInt TargetCharacter;
    public SharedInt talkId;
    public SharedInt nextTalkEventId;
    public SharedBool displayFunction;
    public SharedBool isSimpleTalk;
    public SharedBool isStopMove;
    public SharedBool faceTarget;

    void CharacterStartMoveAction()
    {
        StartCharacterMove startCharacterMove = new StartCharacterMove
        {
            characterId = characterId.Value
        };
        GameActionManager.instance.QueueAction(startCharacterMove);
    }
    public override void OnStart()
    {
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (TargetCharacter == null|| TargetCharacter.IsNull())
        {
            TargetCharacter= (SharedInt)Owner.GetVariable("TargetCharacter");
        }
        if (nextTalkEventId == null || nextTalkEventId.IsNull())
        {
            nextTalkEventId = (SharedInt)Owner.GetVariable("NextTalkEventId");
        }

        if (faceTarget.Value)
        {
            Character character = CharacterManager.instance.GetCharacter(TargetCharacter.Value);
            if (character != null)
            {
                SetTargetDirection setTargetDirection = new SetTargetDirection
                {
                    characterId = characterId.Value,
                    targetCoordinate = character.coordinate
                };
                GameActionManager.instance.QueueAction(setTargetDirection);
            }
        }

        if (isStopMove.Value)
        {
            StopCharacterMove stopCharacterMove = new StopCharacterMove
            {
                characterId = characterId.Value
            };
            GameActionManager.instance.QueueAction(stopCharacterMove);
        }
        if (isSimpleTalk.Value)
        {
            SimpleTalk simpleTalk = new SimpleTalk
            {
                characterId = characterId.Value,
                talkId = talkId.Value,
                endAction=isStopMove.Value? CharacterStartMoveAction:null
            };
            GameActionManager.instance.QueueAction(simpleTalk);
        }
        else
        {
            Talk talk = new Talk
            {
                characterId = characterId.Value,
                talkId = talkId.Value,
                displayFunction = displayFunction.Value,
                nextTalkEventId = nextTalkEventId.Value,
                endAction = isStopMove.Value ? CharacterStartMoveAction : null
            };
            GameActionManager.instance.QueueAction(talk);
        }
       

        taskStatus = TaskStatus.Success;
    }
   

    TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}