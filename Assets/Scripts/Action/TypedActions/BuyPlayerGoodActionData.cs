// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - BuyPlayerGood
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/BuyPlayerGood")]
public class BuyPlayerGoodActionData : GameActionBaseData
{
        public int storeCounterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new BuyPlayerGood
        {
                storeCounterId = this.storeCounterId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (target != 0 && target != int.MinValue) action.storeCounterId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
