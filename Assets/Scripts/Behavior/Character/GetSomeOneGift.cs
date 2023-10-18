using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/Character")]
[TaskName("获得某人的礼物")]
public class GetSomeOneGift : Action
{
    private SharedInt characterId;
    TaskStatus taskStatus;
    private bool addAction = false;
    public override void OnAwake()
    {
        base.OnAwake();
        taskStatus = TaskStatus.Failure;
    }
    public override void OnStart()
    { 
        if (characterId == null || characterId.IsNull())
        {
            characterId = Owner.GetVariable("CharacterId") as SharedInt; 
        }
        if (!addAction)
        {
            GameActionManager.instance.AddListener<GiveGift>(GiveGift);
            addAction = true;
        }
    }
    void GiveGift(GiveGift giveGift)
    {
        if (giveGift.receiveCharacter == characterId.Value)
        {
            //Character character = CharacterManager.instance.GetCharacter(characterId.Value);
            Talk talk = new Talk
            {
                characterId = characterId.Value,
                talkId = GameCommon.defaultGiftTalk,
                displayFunction = false,
                endAction = () =>
                {
                    StartCharacterMove StartCharacterMove = new StartCharacterMove
                    {
                        characterId = characterId.Value,
                    };
                    GameActionManager.instance.QueueAction(StartCharacterMove);
                }
            };
            GameActionManager.instance.QueueAction(talk);
            taskStatus = TaskStatus.Success;
        }
    }
    public override void OnReset()
    {
        addAction = false;
        characterId = null;
        taskStatus = TaskStatus.Failure;
        GameActionManager.instance.RemoveListener<GiveGift>(GiveGift);
        base.OnReset();
    }
   
    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}