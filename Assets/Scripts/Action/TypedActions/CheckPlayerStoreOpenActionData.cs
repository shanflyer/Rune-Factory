// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - CheckPlayerStoreOpen
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/CheckPlayerStoreOpen")]
public class CheckPlayerStoreOpenActionData : GameActionBaseData
{


    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CheckPlayerStoreOpen
        {

        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
