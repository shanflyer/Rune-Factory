// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ShowMultiPackagePanel
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ShowMultiPackagePanel")]
public class ShowMultiPackagePanelActionData : GameActionBaseData
{
        public int packageId0;
        public int packageId1;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ShowMultiPackagePanel
        {
                packageId0 = this.packageId0,
                packageId1 = this.packageId1
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.packageId0 = source;
            if (target != 0 && target != int.MinValue) action.packageId1 = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
