// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RefreshOperateCharacters
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RefreshOperateCharacters")]
public class RefreshOperateCharactersActionData : GameActionBaseData
{


    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshOperateCharacters
        {

        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
