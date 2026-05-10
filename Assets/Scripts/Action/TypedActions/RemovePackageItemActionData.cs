// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RemovePackageItem
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RemovePackageItem")]
public class RemovePackageItemActionData : GameActionBaseData
{
        public int packageId;
        public int itemDataId;
        public int itemCount;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemovePackageItem
        {
                packageId = this.packageId,
                itemDataId = this.itemDataId,
                itemCount = this.itemCount
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
