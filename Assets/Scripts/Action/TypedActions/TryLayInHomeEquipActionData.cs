// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TryLayInHomeEquip
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TryLayInHomeEquip")]
public class TryLayInHomeEquipActionData : GameActionBaseData
{
        public int equipInstanceId;
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryLayInHomeEquip
        {
                equipInstanceId = this.equipInstanceId,
                characterId = this.characterId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
