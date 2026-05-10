// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RefreshShopLevel
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RefreshShopLevel")]
public class RefreshShopLevelActionData : GameActionBaseData
{
        public string shopName;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshShopLevel
        {
                shopName = this.shopName
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
