// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TryRecycleCharacterEmote
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TryRecycleCharacterEmote")]
public class TryRecycleCharacterEmoteActionData : GameActionBaseData
{
        public new int id;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryRecycleCharacterEmote
        {
                id = this.id
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
