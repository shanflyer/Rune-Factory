// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - PlayerTalkItem
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/PlayerTalkItem")]
public class PlayerTalkItemActionData : GameActionBaseData
{
        public int displayTime;
        public int characterId;
        public int ItemId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new PlayerTalkItem
        {
                displayTime = this.displayTime,
                characterId = this.characterId,
                ItemId = this.ItemId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.ItemId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
