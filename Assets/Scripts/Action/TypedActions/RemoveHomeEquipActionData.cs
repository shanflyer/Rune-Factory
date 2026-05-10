// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RemoveHomeEquip
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RemoveHomeEquip")]
public class RemoveHomeEquipActionData : GameActionBaseData
{
        public int characterId;
        public int instanceId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemoveHomeEquip
        {
                characterId = this.characterId,
                instanceId = this.instanceId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
