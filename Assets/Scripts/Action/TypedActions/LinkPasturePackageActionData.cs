// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - LinkPasturePackage
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/LinkPasturePackage")]
public class LinkPasturePackageActionData : GameActionBaseData
{
        public int pastureInstance;
        public int foodPackage;
        public int waterPackage;
        public int productPackage;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new LinkPasturePackage
        {
                pastureInstance = this.pastureInstance,
                foodPackage = this.foodPackage,
                waterPackage = this.waterPackage,
                productPackage = this.productPackage
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
