// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - AddPlayerGold
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/AddPlayerGold")]
public class AddPlayerGoldActionData : GameActionBaseData
{
        public int value;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new AddPlayerGold
        {
                value = this.value
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
