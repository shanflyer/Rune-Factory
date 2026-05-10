// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - TryGiveGiftOpenPackage
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/TryGiveGiftOpenPackage")]
public class TryGiveGiftOpenPackageActionData : GameActionBaseData
{
        public int fromCharacterId;
        public int toCharacterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new TryGiveGiftOpenPackage
        {
                fromCharacterId = this.fromCharacterId,
                toCharacterId = this.toCharacterId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.fromCharacterId = source;
            if (target != 0 && target != int.MinValue) action.toCharacterId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
