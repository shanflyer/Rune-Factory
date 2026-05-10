// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - CreatRuntimePackage
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/CreatRuntimePackage")]
public class CreatRuntimePackageActionData : GameActionBaseData
{
        public new string name;
        public int instanceId;
        public int caseCount;
        public bool itemPackage;
        public Vector2Int key;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new CreatRuntimePackage
        {
                name = this.name,
                instanceId = this.instanceId,
                caseCount = this.caseCount,
                itemPackage = this.itemPackage,
                key = this.key
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
