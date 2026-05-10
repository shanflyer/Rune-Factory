// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TryVisitShop
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TryVisitShop")]
public class TryVisitShopActionData : GameActionBaseData
{
        public string ShopName;
        public int CharacterId;
        public int ShopObjId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryVisitShop
        {
                ShopName = this.ShopName,
                CharacterId = this.CharacterId,
                ShopObjId = this.ShopObjId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.CharacterId = source;
            if (target != 0 && target != int.MinValue) action.ShopObjId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
