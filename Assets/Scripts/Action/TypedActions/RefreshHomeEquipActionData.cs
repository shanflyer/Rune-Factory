// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RefreshHomeEquip
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RefreshHomeEquip")]
public class RefreshHomeEquipActionData : GameActionBaseData
{
        public int equipInstanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RefreshHomeEquip
        {
                equipInstanceId = this.equipInstanceId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
