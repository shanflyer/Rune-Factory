// ────────────────────────────────────────────────────
// 自动生成: GameAction 强类型配置 - StartFishing
// ────────────────────────────────────────────────────
using UnityEngine;

[CreateAssetMenu(menuName = "GameAction/StartFishing")]
public class StartFishingActionData : GameActionBaseData
{
        public int mapItemId;
        public int characterId;

    public override GameAction CreateAction(
        int source = 0, int target = 0, int value = -1,
        SetResult setResult = null, SetValue setValue = null,
        bool immediately = false)
    {
        var action = new StartFishing
        {
                mapItemId = this.mapItemId,
                characterId = this.characterId
        
        };

        action.setValue = setValue;
        action.setResult = setResult;
            if (source != 0 && source != int.MinValue) action.characterId = source;
            if (target != 0 && target != int.MinValue) action.mapItemId = target;

        GameActionManager.instance.QueueAction(action, immediately);
        return action;
    }
}
