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
    public SharedInt talkId;
    public SharedBool displayFunction;
    public SharedBool isSimpleTalk;
    public SharedBool isStopMove;

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