// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SwitchAutoStore
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SwitchAutoStore")]
public class SwitchAutoStoreActionData : GameActionBaseData
{
        public bool isAuto;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SwitchAutoStore
        {
                isAuto = this.isAuto
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
