using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Game/Character")]
[TaskName("赠送某人的礼物")]
public class SetSomeOneGift : Action
{
    public SharedInt characterId;
    public SharedInt gift;
    public SharedInt targetCharacterId;
    private TaskStatus taskStatus;
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

    private void GiveGift(GiveGift giveGift)
    {
        if (giveGift.receiveCharacter == characterId.Value)
        {
             
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