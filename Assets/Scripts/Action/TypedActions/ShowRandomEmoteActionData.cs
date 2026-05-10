// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ShowRandomEmote
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ShowRandomEmote")]
public class ShowRandomEmoteActionData : GameActionBaseData
{
        public new int id;
        public int randomId;
        public int showTime;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ShowRandomEmote
        {
                id = this.id,
                randomId = this.randomId,
                showTime = this.showTime
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
