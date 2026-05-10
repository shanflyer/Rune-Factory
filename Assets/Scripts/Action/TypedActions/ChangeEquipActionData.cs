// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - ChangeEquip
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/ChangeEquip")]
public class ChangeEquipActionData : GameActionBaseData
{
        public int characterId;
        public int outPackageId;
        public int itemId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new ChangeEquip
        {
                characterId = this.characterId,
                outPackageId = this.outPackageId,
                itemId = this.itemId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.outPackageId = target;
            if (value != -1) action.itemId = value;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
