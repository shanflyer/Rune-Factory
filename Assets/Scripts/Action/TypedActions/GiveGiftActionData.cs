// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - GiveGift
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/GiveGift")]
public class GiveGiftActionData : GameActionBaseData
{
        public int giveCharacter;
        public int receiveCharacter;
        public int giftId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new GiveGift
        {
                giveCharacter = this.giveCharacter,
                receiveCharacter = this.receiveCharacter,
                giftId = this.giftId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.giveCharacter = source;
            if (target != 0 && target != int.MinValue) action.receiveCharacter = target;
            if (value != -1) action.giftId = value;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
