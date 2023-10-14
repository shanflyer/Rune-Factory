using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.Mathematics; 

[TaskCategory("Game/PlayerStore")]
[TaskName("购买玩家上架商品")]
public class TempBuyPlayerGood : Action
{
    public SharedInt selectStoreCounterId;
    private SharedInt characterId;
    public override void OnStart()
    {
        taskStatus = TaskStatus.Running;
        if (selectStoreCounterId == null)
        {
            selectStoreCounterId = (SharedInt)Owner.GetVariable("SelectStoreCounterId");
             
        }
        if (characterId == null)
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (selectStoreCounterId == null|| characterId == null)
        {
            taskStatus = TaskStatus.Failure;
            return;
        }
        TryBuyPlayerGood tryBuyPlayerGood = new TryBuyPlayerGood
        {
            characterId = characterId.Value,
            storeCounterId = selectStoreCounterId.Value,
            setResult= BuySuccess
        };
        GameActionManager.instance.QueueAction(tryBuyPlayerGood);
    } 
    void BuySuccess(bool success)
    {
        if (success)
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