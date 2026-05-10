// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SetPlayerStoreOpen
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SetPlayerStoreOpen")]
public class SetPlayerStoreOpenActionData : GameActionBaseData
{
        public bool open;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SetPlayerStoreOpen
        {
                open = this.open
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
