// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - CreatStoreCounter
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/CreatStoreCounter")]
public class CreatStoreCounterActionData : GameActionBaseData
{
        public int itemInstanceId;
        public int storeDataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatStoreCounter
        {
                itemInstanceId = this.itemInstanceId,
                storeDataId = this.storeDataId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
