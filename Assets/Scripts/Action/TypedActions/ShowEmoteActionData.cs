// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ShowEmote
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ShowEmote")]
public class ShowEmoteActionData : GameActionBaseData
{
        public new int id;
        public int emoteId;
        public int showTime;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ShowEmote
        {
                id = this.id,
                emoteId = this.emoteId,
                showTime = this.showTime
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
