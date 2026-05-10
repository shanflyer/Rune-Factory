// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - SortPackageItem
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/SortPackageItem")]
public class SortPackageItemActionData : GameActionBaseData
{
        public int packageId;
        public int itemId;
        public int index;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new SortPackageItem
        {
                packageId = this.packageId,
                itemId = this.itemId,
                index = this.index
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
