// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - RemovePlayerPackageItem
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/RemovePlayerPackageItem")]
public class RemovePlayerPackageItemActionData : GameActionBaseData
{
        public int characterId;
        public int itemDataId;
        public int itemCount;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new RemovePlayerPackageItem
        {
                characterId = this.characterId,
                itemDataId = this.itemDataId,
                itemCount = this.itemCount
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.itemDataId = target;
            if (value != -1) action.itemCount = value;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
