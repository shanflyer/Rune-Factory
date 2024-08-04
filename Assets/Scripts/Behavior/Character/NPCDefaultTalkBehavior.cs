using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics;


[TaskCategory("Game/Character")]
[TaskName("NPC对话")]
public class NPCDefaultTalkBehavior : Action
{
    [SerializeField]
    private SharedInt characterId;
    public SharedInt TargetCharacter; 
    public SharedInt nextTalkEventId;
    public SharedBool displayFunction; 
    public SharedBool isStopMove;
    public SharedBool faceTarget;
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
        if (characterId == null || characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (NPCManager.instance.GetNPCFormInstance(characterId.Value, out var npc))
        { 
            int talkId = npc.GetTalkId();
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
            Talk talk = new Talk
            {
                characterId = characterId.Value,
                talkId = talkId,
                displayFunction = displayFunction.Value,
                nextTalkEventId = nextTalkEventId.Value,
                endAction = isStopMove.Value ? () =>
                {
                    CharacterStartMoveAction();
                    RemoveEvent();
                }
                :
                    () => { RemoveEvent(); }
            };
            GameActionManager.instance.QueueAction(talk, true);

        }
        taskStatus = TaskStatus.Success;
    }
    private TaskStatus taskStatus;
    public override TaskStatus OnUpdate()
    {
      

        return taskStatus;
    }
}