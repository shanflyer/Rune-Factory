// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RemovePackageItemInstance
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RemovePackageItemInstance")]
public class RemovePackageItemInstanceActionData : GameActionBaseData
{
        public int packageId;
        public int itemInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemovePackageItemInstance
        {
                packageId = this.packageId,
                itemInstanceId = this.itemInstanceId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
