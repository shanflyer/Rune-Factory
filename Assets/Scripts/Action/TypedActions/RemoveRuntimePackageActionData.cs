// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RemoveRuntimePackage
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RemoveRuntimePackage")]
public class RemoveRuntimePackageActionData : GameActionBaseData
{
        public Vector2Int key;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemoveRuntimePackage
        {
                key = this.key
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
