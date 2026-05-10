// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RemovePackage
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RemovePackage")]
public class RemovePackageActionData : GameActionBaseData
{
        public int packageDataId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemovePackage
        {
                packageDataId = this.packageDataId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
