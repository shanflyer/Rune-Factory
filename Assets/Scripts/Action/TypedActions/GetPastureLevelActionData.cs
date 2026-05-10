// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - GetPastureLevel
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/GetPastureLevel")]
public class GetPastureLevelActionData : GameActionBaseData
{
        public int pastureId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new GetPastureLevel
        {
                pastureId = this.pastureId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
