// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ShopBuySuccess
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ShopBuySuccess")]
public class ShopBuySuccessActionData : GameActionBaseData
{
        public int buyCount;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ShopBuySuccess
        {
                buyCount = this.buyCount
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
